using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public class Templates
    {
        public const string CLASS_TEMPLATE = @"
using FluentValidation.Results;
using Newtonsoft.Json;
using @SolutionName@.Common;
using @SolutionName@.Core.Database.Identity;
using @SolutionName@.Core.Models;
using @SolutionName@.Core.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace @SolutionName@.Core.Database.Tables
{
    public class @ClassName@:BaseEntity<@TID@>
    {
        @NavigationProps@
        @ForeignKeys@

        @ValidatorType@ _Validator;
        private @ValidatorType@ Validator
        {
            get
            {
                _Validator ??= new @ValidatorType@();
                return _Validator;
            }
        }


        private @ClassName@()
        {
            @PrivateConst@
        }

       @Props@
	   
        public @ClassName@(@ClassName@ModifyModel model)
        {
            ValidationResult result = Validator.Validate(model);
            if (!result.IsValid)
            {
                var messages = result.Errors.Select(e => e.ErrorMessage);
                throw new HttpException(LoggingEvents.Constractor_ERROR, JsonConvert.SerializeObject(messages));
            }

            @PublicConst@

            CreatedDate = DateTime.UtcNow;

        }

        public @ClassName@ Update(@ClassName@ModifyModel model)
        {
            ValidationResult result = Validator.Validate(model);
            if (!result.IsValid)
            {
                var messages = result.Errors.Select(e => e.ErrorMessage);
                throw new HttpException(LoggingEvents.Constractor_ERROR, JsonConvert.SerializeObject(messages));
            }

           @Update@

            ModifiedDate = DateTime.UtcNow;
            return this;
        }

       

        public @ClassName@ Delete()
        {
            IsActive = false;
            ModifiedDate = DateTime.UtcNow;
            return this;
        }

        @ChildFunctions@
    }
}

    ";
        public const string MODELS_TEMPLATE = @"
using @SolutionName@.Core.Database.Identity;
using @SolutionName@.Core.Database.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace @SolutionName@.Core.Models
{
    public class @ClassName@ReadModel
    {
        public @TID@ Id {get; set;}
        @Props@
        @ForeignKeys@
      
       
    }
    public class @ClassName@ModifyModel
    {
        public @TID@ Id {get; set;}
        @Props@
      
        @NavigationProps@
        @ForeignKeys@
    }
}

";
        public const string VALIDATOR_TEMPLATE = @"
using FluentValidation;
using @SolutionName@.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace @SolutionName@.Core.Validations
{
    public class @ClassName@Validator: AbstractValidator<@ClassName@ModifyModel>
    {
        public @ClassName@Validator()
        {
            

        }
    }
}

";
        public const string DBCONTEXT_SET_TEMPLATE = @"
public virtual DbSet<@ClassName@> @ClassNames@ { get; set; }";
        public const string DBCONTEXT_OnModelCreating_TEMPLATE = @"modelBuilder.ApplyConfiguration(new  @ClassName@EntityConfiguration());";
        public const string RELATION_CONFIG_TEMPLATE = @"
 modelBuilder.HasMany(e => e.@Children@).WithOne(e => e.@Parent@).HasForeignKey(e => e.@ForeignKey@).OnDelete(DeleteBehavior.Cascade);
";
        public const string CHILED_FUNCTIONS_TEMPLATE = @"
    public @ClassName@ Add@RelatedName@(@RelatedName@ModifyModel model)
        {

            @SetRelatedName@ ??= new HashSet<@RelatedName@>();
            @SetRelatedName@.Add(new @RelatedName@(model));

            return this;
        }
        public @ClassName@ Update@RelatedName@(@RelatedName@ModifyModel model)
        {
            if (model.Id <= 0)
            {
                throw new HttpException(LoggingEvents.CREATE_ITEM, ""InvalidData"");
            }


        @SetRelatedName@ ??= new HashSet<@RelatedName@>();
                var entity = @SetRelatedName@.First(e => e.Id == model.Id);
        entity.Update(model);

            return this;
        }
    public @ClassName@ Remove@RelatedName@(long id)
        {
            if (id <= 0)
            {
                throw new HttpException(LoggingEvents.CREATE_ITEM, ""InvalidData"");
        }


        @SetRelatedName@ ??= new HashSet<@RelatedName@>();
        var entity = @SetRelatedName@.First(e => e.Id == id);
        @SetRelatedName@.Remove(entity);

        return this;
        }

        public @ClassName@ Update@SetRelatedName@(HashSet<@RelatedName@ModifyModel> @_SetRelatedName@)
        {
            var currentIds = @_SetRelatedName@.Select(e => e.Id).ToList();
            var removed = @SetRelatedName@.Where(e => !currentIds.Contains(e.Id)).ToList();

            var updated = @_SetRelatedName@.Where(e => e.Id > 0).ToList();

            var inserted = @_SetRelatedName@.Where(e => e.Id == 0).ToList();

            removed.ForEach(e => Remove@RelatedName@(e.Id));
            inserted.ForEach(e => Add@RelatedName@(e));
            if (updated.Any())
            {
                updated.ForEach(e => Update@RelatedName@(e));
            }

        return this;
    }
";
        public const string AUTO_MAPPER_TEMPLATE = @" CreateMap<@ClassName@, @ClassName@ReadModel>().IgnoreAllPropertiesWithAnInaccessibleSetter().IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
            CreateMap<@ClassName@, @ClassName@ModifyModel>().IgnoreAllPropertiesWithAnInaccessibleSetter().IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
";
        public const string SERVICE_TEMPALTE = @"
using AutoMapper;
using ElmahCore;
using @SolutionName@.Core.Database.Tables;
using @SolutionName@.Core.Enums;
using @SolutionName@.Core.Models;
using @SolutionName@.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace @SolutionName@.Services
{
   

    public class @ClassName@Service
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public @ClassName@Service(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public ResultViewModel<List<@ClassName@ReadModel>> All(QueryViewModel<@ClassName@ReadModel> query)
        {
            try
            {
                query.Order = string.IsNullOrEmpty(query.Order) ? ""Id Desc"" : query.Order;
                var data = _context.@ClassNames@.AsQueryable();
                if (!string.IsNullOrEmpty(query.Filter))
                {
                    data = data.Where(query.Filter);
                }

                data = data.OrderBy(query.Order);

                var page = (query.PageNumber <= 0 ? data :
                           data.Skip((query.PageNumber - 1) * query.PageSize)
                           .Take(query.PageSize))
                           .ToList();



                return new ResultViewModel<List<@ClassName@ReadModel>>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<List<@ClassName@ReadModel>>(page),
                    Filter = query.Filter,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    Total = data.Count()
                };
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<List<@ClassName@ReadModel>>
                {
                    IsSuccess = false,
                    Data = null,
                    Filter = query.Filter,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    Total = 0,
                    Exception = ex
                };
            }
        }

        public ResultViewModel<@ClassName@ModifyModel> Find(Expression<Func<@ClassName@, bool>> predicate)
        {
            try
            {
                var data = _context.@ClassNames@.FirstOrDefault(predicate);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<@ClassName@ModifyModel>(data),
                    Total = 1
                };
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = false,
                    Data = null,
                    Total = 0,
                    Exception = ex
                };
            }
        }
        public ResultViewModel<@ClassName@ModifyModel> Find(int id)
        {
            try
            {
                var data = _context.@ClassNames@.Find(id);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<@ClassName@ModifyModel>(data),
                    Total = 1
                };
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = false,
                    Data = null,
                    Total = 0,
                    Exception = ex
                };
            }
        }

        
        public ResultViewModel<@ClassName@ModifyModel> Add(@ClassName@ModifyModel model)
        {
            try
            {
                var @_ClassName@ = new @ClassName@(model);
                //TODO: Handel chiledren
                var entity = _context.@ClassNames@.Add(@_ClassName@);
                _ = _context.SaveChanges();
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<@ClassName@ModifyModel>(entity.Entity),
                    Total = 1
                };

            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = false,
                    Data = null,
                    Total = 0,
                    Exception = ex
                };
            }
        }

        public ResultViewModel<@ClassName@ModifyModel> Update(@ClassName@ModifyModel model)
        {
            try
            {
                var @_ClassName@ = _context.@ClassNames@.Find(model.Id);
                if (@_ClassName@ == null)
                {
                    var exp = new KeyNotFoundException($""item number {model.Id} does not Exist"");
                    ElmahExtensions.RiseError(exp);
                    return new ResultViewModel<@ClassName@ModifyModel>
                    {
                        IsSuccess = false,
                        Data = null,
                        Total = 0,
                        Exception = exp,
                        Messages = new List<string> { ""ItemNotFound"" } 
                    };
                }

                //TODO: Hanlde Update
                var entity = @_ClassName@.Update(model);
               
                _ = _context.SaveChanges();
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<@ClassName@ModifyModel>(entity),
                    Total = 1
                };

            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = false,
                    Data = null,
                    Total = 0,
                    Exception = ex
                };
            }
        }

        public ResultViewModel<@ClassName@ModifyModel> Delete(int id)
        {
            try
            {
                var @_ClassName@ = _context.@ClassNames@.Find(id);
                if (@_ClassName@ == null)
                {
                    var exp = new KeyNotFoundException($""item number {id} does not Exist"");
                    ElmahExtensions.RiseError(exp);
                    return new ResultViewModel<@ClassName@ModifyModel>
                    {
                        IsSuccess = false,
                        Data = null,
                        Total = 0,
                        Exception = exp,
                        Messages = new List<string> { ""ItemNotFound"" } 
                    };
                }
                var entity = @_ClassName@.Delete();
                _ = _context.SaveChanges();
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = true,
                    Data = _mapper.Map<@ClassName@ModifyModel>(entity),
                    Total = 1
                };

            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(ex);
                return new ResultViewModel<@ClassName@ModifyModel>
                {
                    IsSuccess = false,
                    Data = null,
                    Total = 0,
                    Exception = ex
                };
            }
        }


    }
}

";
        public const string API_CONTROLLER_TEMPLATE = @"
using System;
using System.Collections.Generic;
using System.Linq;
using ElmahCore;
using @SolutionName@.Common;
using @SolutionName@.Core.Database.Tables;
using @SolutionName@.Core.Models;
using @SolutionName@.Services;
using Microsoft.AspNetCore.Mvc;

namespace @SolutionName@.AppAPI.Controllers
{
    [Route(""api/[controller]"")]
    [ApiController]
    public class @ClassNames@Controller : BaseController
    {
        private readonly @ClassName@Service _@_ClassName@Service;
        private readonly Translator _translator;

        public @ClassNames@Controller(@ClassName@Service @_ClassName@Service, Translator translator):base(translator)
        {
            _@_ClassName@Service = @_ClassName@Service;
            _translator = translator;
        }



        [HttpGet]
        public ActionResult<ResultViewModel<List<@ClassName@ReadModel>>> GetAll([FromQuery] QueryViewModel<@ClassName@ReadModel> query)
        {
            var result = new ResultViewModel<List<@ClassName@ReadModel>>();
            try
            {
                query.UserId = User.Claims.FirstOrDefault(c => c.Type == ""UserId"")?.Value.ToNullableLong();
               //  query.CompanyId = User.Claims.FirstOrDefault(c => c.Type == ""CompanyId"")?.Value.ToNullableLong();
                var data = _@_ClassName@Service.All(query);
                data.Messages= data.Messages.Select(m => _translator.Translate(m, Language)).ToList();
                return Ok(data);

            }
            catch (HttpException ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                return Ok(HandleHttpException<List<@ClassName@ReadModel>>(ex));
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                result.IsSuccess = false;
                result.Exception = ex;
                return Ok(result);
            }
        }

        [HttpGet(""{id}"")]
        public ActionResult<ResultViewModel<@ClassName@ModifyModel>> GetOne(int id)
        {
            var result = new ResultViewModel<@ClassName@ModifyModel>();
            try
            {
                var data = _@_ClassName@Service.Find(id);
                data.Messages = data.Messages.Select(m => _translator.Translate(m, Language)).ToList();
                return Ok(data);

            }
            catch (HttpException ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                return Ok(HandleHttpException<@ClassName@ModifyModel>(ex));
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                result.IsSuccess = false;
                result.Exception = ex;
                return Ok(result);
            }
        }

        //TODO: Handle Children

        [HttpPost]
        public ActionResult<ResultViewModel<@ClassName@ModifyModel>> Post@ClassName@([FromBody] @ClassName@ModifyModel model)
        {
            var result = new ResultViewModel<@ClassName@>();
            try
            {
                var data = _@_ClassName@Service.Add(model);
                data.Messages = data.Messages.Select(m => _translator.Translate(m, Language)).ToList();
                return Ok(data);

            }
            catch (HttpException ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                return Ok(HandleHttpException<@ClassName@ModifyModel>(ex));
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                result.IsSuccess = false;
                result.Exception = ex;
                return Ok(result);
            }
        }

        [HttpPut(""{id}"")]
        public ActionResult<ResultViewModel<@ClassName@ModifyModel>> Put@ClassName@(int id, [FromBody] @ClassName@ModifyModel model)
        {
            var result = new ResultViewModel<@ClassName@>();
            try
            {
                if (model.Id != id)
                {
                    result.IsSuccess = false;
                    result.Messages.Add(_translator.Translate(""InvalidData"", Language));
                    return Ok(result);
                }
                var data = _@_ClassName@Service.Update(model);
                data.Messages = data.Messages.Select(m => _translator.Translate(m, Language)).ToList();
                return Ok(data);

            }
            catch (HttpException ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                return Ok(HandleHttpException<@ClassName@ModifyModel>(ex));
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                result.IsSuccess = false;
                result.Exception = ex;
                return Ok(result);
            }
        }

        [HttpDelete(""{id}"")]
        public ActionResult<ResultViewModel<@ClassName@ModifyModel>> Delete@ClassName@(int id)
        {
            var result = new ResultViewModel<@ClassName@ModifyModel>();
            try
            {
                var data = _@_ClassName@Service.Delete(id);
                data.Messages = data.Messages.Select(m => _translator.Translate(m, Language)).ToList();
                return Ok(data);

            }
            catch (HttpException ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                return Ok(HandleHttpException<@ClassName@ModifyModel>(ex));
            }
            catch (Exception ex)
            {

                ElmahExtensions.RiseError(HttpContext, ex);
                result.IsSuccess = false;
                result.Exception = ex;
                return Ok(result);
            }
        }


    }

}
";
        public const string ENTITY_CONFIGURATIONS_TEMPLATE = @"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using @SolutionName@.Common;
using @SolutionName@.Core.Database.Identity;
using @SolutionName@.Core.Models;
using @SolutionName@.Core.Validations;
using @SolutionName@.Core.Database.Tables;
namespace @SolutionName@.EntityFrameworkCore.EntityConfigurations
{
    public class @ClassName@EntityConfiguration : IEntityTypeConfiguration<@ClassName@>
    {
        private readonly string _schema;
        public @ClassName@EntityConfiguration(string schema=""dbo"")
        {
            _schema = schema;
        }
    public void Configure(EntityTypeBuilder<@ClassName@> modelBuilder)
    {
        if (!string.IsNullOrWhiteSpace(_schema))
            modelBuilder.ToTable(""@ClassNames@"", _schema);
        modelBuilder.HasKey(e => e.Id);
        //modelBuilder.Property(e => e.Id).ValueGeneratedNever();
          modelBuilder.HasQueryFilter(e => e.IsActive != false);
          modelBuilder.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql(""((1))"");
          modelBuilder.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql(""(getdate())"");
        @RelationsConfig@
        }
}
}";
    }
}
