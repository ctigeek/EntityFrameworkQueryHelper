using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace QueryHelper
{
    public class TestController : ApiController, IDisposable
    {
        private readonly TestDbContext repository;
        private static readonly EFQueryHelper<ThePoco> queryHelper = new EFQueryHelper<ThePoco>();
        
        public TestController() : this(new TestDbContext())
        {
        }

        internal TestController(TestDbContext repository)
        {
            this.repository = repository ?? new TestDbContext();
        }

        public new void Dispose()
        {
            repository.Dispose();
            base.Dispose();
        }

        public async Task<IHttpActionResult> Get(string id, [FromUri] QueryOptions options)
        {
            try
            {
                var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Path);
                options.OrderBy = options.OrderBy ?? "TimeStamp";                                
                
                var results = await GetQueryResults(id, options);
                foreach (var result in results)
                {
                    result.Uri = baseUrl + "/" + result.Id;
                }
                return Ok(results);
            }
            catch (ArgumentException argex)
            {
                return BadRequest(argex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        
        private async Task<List<ThePoco>> GetQueryResults(string id, QueryOptions options) {
            
            var query = repository.TableMappingSet.Where(al => al.Id == id);

            var whereExpression =  queryHelper.CreateFilterExpression(options.Where);
            if (whereExpression != null)
            {
                query = query.Where(whereExpression);
            }

            var searchExpression = queryHelper.CreateSearchExpression(options.Search);
            if (searchExpression != null)
            {
                query = query.Where(searchExpression);
            }

            if (string.IsNullOrEmpty(options.OrderBy) || options.OrderBy.Equals(nameof(ThePoco.TimeStamp), StringComparison.InvariantCultureIgnoreCase))
            {
                query = options.OrderDesc
                    ? query.OrderByDescending(a => a.TimeStamp)
                    : query.OrderBy(a => a.TimeStamp);
            }
            else
            {
                query = queryHelper.AddOrderByClause(query, options.OrderBy, options.OrderDesc);
            }
            query = query.Skip(options.Offset)
                .Take(options.Limit);
                
            var results = await query.ToListAsync();
        }
    }
}