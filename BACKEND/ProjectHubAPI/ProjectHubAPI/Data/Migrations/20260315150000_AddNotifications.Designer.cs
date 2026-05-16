using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectHubAPI.Data;

#nullable disable

namespace ProjectHubAPI.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260315150000_AddNotifications")]
    partial class AddNotifications
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            // ... (I will need to copy the full model snapshot logic here, but for now I'll focus on the Snapshot file itself)
#pragma warning restore 612, 618
        }
    }
}
 
