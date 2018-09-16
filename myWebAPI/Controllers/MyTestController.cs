using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class MyTestController : ControllerBase
    {
        private readonly ICityNameService _cityNameService;

        public MyTestController(ICityNameService cityNameService)
        {
            _cityNameService = cityNameService;
        }
        [HttpPut("myupdate")]
        public async Task<ApiResponse> Update(UpdateRequest updateRequest)
        {
            await _cityNameService.UpdateAsync(new CityName { Id = updateRequest.Id, CName = updateRequest.Name });
            var res = new ApiResponse()
            {
                StatusCode = "200",
                Error = ""
            };
            return await Task.FromResult(res);
        }

        [HttpDelete("mydelete")]
        public async Task<ApiResponse> Delete(int id)
        {
            await _cityNameService.DeleteAsync(id);
            var res = new ApiResponse()
            {
                StatusCode = "200",
                Error = ""
            };
            return await Task.FromResult(res);
        }
        [HttpGet("{name}")]
        public async Task<ApiResponse<IEnumerable<CityName>>> Select(string name)
        {
            var result = await _cityNameService.SelectAsync(name);
            var res = new ApiResponse<IEnumerable<CityName>>
            {
                Error = "",
                StatusCode = "200",
                Data = result
            };
            return await Task.FromResult(res);
        }

        [HttpPost("mycreate")]
        public async Task<ApiResponse> Create(UpdateRequest request)
        {
            await _cityNameService.AddAsync(new CityName { Id = request.Id, CName = request.Name });
            var res = new ApiResponse()
            {
                Error = "",
                StatusCode = "200"
            };
            return await Task.FromResult(res);
        }
    }


    public interface ICityNameService
    {
        Task AddAsync(CityName cityName);
        Task DeleteAsync(int id);
        Task UpdateAsync(CityName cityName);
        Task<IEnumerable<CityName>> SelectAsync(string name);
    }

    public class CityNameService : ICityNameService
    {
        private readonly IBaseDao _baseDao;
        public CityNameService(IBaseDao baseDao)
        {
            _baseDao = baseDao;
        }
        async Task ICityNameService.AddAsync(CityName cityName)
        {
            string sql = "insert into dbo.cityname values(@id,@name)";
            var paras = new Dictionary<string, object>()
                {
                    {"id", cityName.Id },
                    {"name",cityName.CName }
                };
            await _baseDao.ExecuteAsync(sql, paras);
        }

        async Task ICityNameService.DeleteAsync(int id)
        {
            string sql = "delete dbo.cityname where id=@id";
            var paras = new Dictionary<string, object>()
                {
                    {"id", id }
                };
            await _baseDao.ExecuteAsync(sql, paras);
        }

        async Task<IEnumerable<CityName>> ICityNameService.SelectAsync(string name)
        {
            var result = new List<CityName>();
            string sql = "select id,CName from dbo.cityname where CName=@name";
            var paras = new Dictionary<string, object>()
            {
                { "name",name}
            };
            return await _baseDao.Query<CityName>(sql, paras);
        }

        async Task ICityNameService.UpdateAsync(CityName cityName)
        {
            string sql = "update dbo.cityname set CName=@name where id=@id";
            var paras = new Dictionary<string, object>()
            {
                { "id",cityName.Id},
                { "name",cityName.CName}
            };
            await _baseDao.ExecuteAsync(sql, paras);
        }
    }

    public class CityName
    {
        public int Id { get; set; }
        public string CName { get; set; }
    }

    public class ApiResponse
    {
        public string StatusCode { get; set; }
        public string Error { get; set; }
    }

    public class ApiResponse<T>
    {
        public string StatusCode { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
    }

    public class UpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}