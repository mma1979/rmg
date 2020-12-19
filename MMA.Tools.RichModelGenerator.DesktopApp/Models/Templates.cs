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
    public class @ClassName@:BaseEntity<long>
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
            IsDeleted = false;
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
        public long Id {get; set;}
        @Props@
        @ForeignKeys@
      
       
    }
    public class @ClassName@ModifyModel
    {
        public long Id {get; set;}
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
        //        public const string DBCONTEXT_OnModelCreating_TEMPLATE = @"

        //modelBuilder.Entity<@ClassName@>(entity =>
        //            {
        //                entity.HasQueryFilter(e => e.IsDeleted != false);
        //                entity.Property(e => e.IsDeleted).HasDefaultValueSql(""((1))"");
        //                entity.Property(e => e.CreatedDate).HasDefaultValueSql(""(getdate())"");
        //                @RelationsConfig@

        //    });
        //";
        public const string DBCONTEXT_OnModelCreating_TEMPLATE = @"modelBuilder.ApplyConfiguration(new  @ClassName@EntityConfiguration());";
        public const string RELATION_CONFIG_TEMPLATE = @"
 modelBuilder.HasMany(e => e.@Children@).WithOne(e => e.@Parent@).HasForeignKey(e => e.@ForeignKey@).OnDelete(DeleteBehavior.Cascade);
";
        public const string CHILED_FUNCTIONS_TEMPLATE = @"
    public @ClassName@ Add@RelatedName@(@RelatedName@ModifyModel model)
        {

            @SetRelatedName@ ??= new HashSet<Project>();
            @SetRelatedName@.Add(new Project(model));

            return this;
        }
        public @ClassName@ Update@RelatedName@(long id, @RelatedName@ModifyModel model)
        {
            if (id <= 0)
            {
                throw new HttpException(LoggingEvents.CREATE_ITEM, ""InvalidData"");
            }


        @SetRelatedName@ ??= new HashSet<@RelatedName@>();
                var entity = @SetRelatedName@.First(e => e.Id == id);
        entity.Update(model);

            return this;
        }
    public @ClassName@ Remove@RelatedName@(long id)
        {
            if (id <= 0)
            {
                throw new HttpException(LoggingEvents.CREATE_ITEM, ""InvalidData"");
        }


        @SetRelatedName@ ??= new HashSet<Project>();
        var entity = @SetRelatedName@.First(e => e.Id == id);
        @SetRelatedName@.Remove(entity);

        return this;
        }

        public @ClassName@ Update@SetRelatedName@(HashSet<@RelatedName@> @_SetRelatedName@)
        {
            var currentIds = @_SetRelatedName@.Select(e => e.Id).ToList();
            var removed = @RelatedName@.Where(e => !currentIds.Contains(e.Id)).ToList();

            var updated = @_SetRelatedName@.Where(e => e.Id > 0).ToList();

            var inserted = @_SetRelatedName@.Where(e => e.Id == 0).ToList();

            removed.ForEach(e => Remove@RelatedName@(e.Id));
            inserted.ForEach(e => Add@RelatedName@(new @RelatedName@ModifyModel(e)));
            if (updated.Any())
            {
                updated.ForEach(e => Update@RelatedName@(new @RelatedName@ModifyModel(e)));
            }

        return this;
    }
";
        public const string ENTITY_CONFIGURATIONS_TEMPLATE = @"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using @SolutionName@.Common;
using @SolutionName@.Core.Database.Identity;
using @SolutionName@.Core.Models;
using @SolutionName@.Core.Validations;
using @SolutionName@.Core.Database.Tables;

namespace @SolutionName@.EntityFramworkCore.EntityConfigurations
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

         modelBuilder.HasQueryFilter(e => e.IsDeleted != true);
        modelBuilder.Property(e => e.IsDeleted).HasDefaultValueSql(""((0))"");
        modelBuilder.Property(e => e.CreatedDate).HasDefaultValueSql(""(getdate())"");
        @RelationsConfig@
        }
}
}";
    }
}
