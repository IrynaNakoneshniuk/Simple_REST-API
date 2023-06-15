
using System.Text.RegularExpressions;
using WebApplication3;

List<Car> cars = new List<Car>() { new Car () { Id=Guid.NewGuid().ToString(),Brand="Audi",Model="TYU", Year= "2011" }, new Car()
{ Id=Guid.NewGuid().ToString(), Brand = "BMW", Model = "KOU", Year = "2011" } };

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    var response= context.Response;
    var request = context.Request;
    var path = context.Request.Path;
    string expressionForGuid = @"^/api/cars/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path== "/api/cars" && request.Method=="GET")
    {
        await GetAllCars(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid)&&request.Method=="GET")
    {
        string id = path.Value?.Split("/")[3];
        await GetCarById(response, id);
    }
    else if (path == "/api/cars" && request.Method == "POST")
    {
        await CreateCar(response,request);
    }
    else if (path == "/api/cars" && request.Method == "PUT")
    {
        await UpdateCar( request, response);
    }
    else if(Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string id = path.Value?.Split("/")[3];
        await DeleteCar(id, response);
    }
    else
    {
        response.ContentType= "text/html; charset=utf-8";
        await response.SendFileAsync("pages/index.html");
    }

});



app.Run();

async Task DeleteCar(string? id, HttpResponse response)
{
    Car? car = cars.FirstOrDefault(x => x.Id == id);
    if (car != null)
    {
        cars.Remove(car);
        await response.WriteAsJsonAsync(car);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Не знайдено!" });
    }
}

async Task GetAllCars(HttpResponse httpResponse)
{
   await httpResponse.WriteAsJsonAsync(cars);
}


async Task CreateCar(HttpResponse httpResponse,HttpRequest httpRequest)
{
    try
    {

        Car ?car = await httpRequest.ReadFromJsonAsync<Car>();
        if (car != null)
        {
            car.Id=Guid.NewGuid().ToString();
            cars.Add(car);

          await  httpResponse.WriteAsJsonAsync(car);
        }
        else
        {
            throw new Exception("Serealization ex");
        }

    }catch(Exception ex)
    {
        httpResponse.StatusCode = 400;
       await  httpResponse.WriteAsJsonAsync(new {message=ex.Message });
    }
}


async Task GetCarById(HttpResponse httpResponse,string id)
{
    Car? car =cars.FirstOrDefault(cars=> cars.Id==id);
    try
    {
        if (car != null)
        {
            await httpResponse.WriteAsJsonAsync(car);
        }
        else
        {
            throw new Exception("404 error");
        }
    }
    catch(Exception ex)
    {
        httpResponse.StatusCode = 404;
        await httpResponse.WriteAsJsonAsync(new { message = ex.Message });
    }
    
}

async Task UpdateCar(HttpRequest httpRequest,HttpResponse httpResponse)
{
    try
    {
        Car? car = await httpRequest.ReadFromJsonAsync<Car>();
        if (car != null)
        {
            Car ?oldCar = cars.FirstOrDefault(car => car.Id == car.Id);
            if (oldCar != null)
            {
                oldCar.Year = car.Year;
                oldCar.Model = car.Model;
                oldCar.Brand = car.Brand;

                await httpResponse.WriteAsJsonAsync(oldCar);
            }
            else
            {
                httpResponse.StatusCode = 404;
                await httpResponse.WriteAsJsonAsync(new { message = "Не знайдено!" });
            }
        }
        else
        {
            throw new Exception("Невірні дані!");
        }
    }
    catch (Exception)
    {
        httpResponse.StatusCode = 400;
        await httpResponse.WriteAsJsonAsync(new { message = "Невірні дані!" });
    }   
}