using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPAComments.CommentsModule.Domain;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Infrastructure.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                guid => CommentId.Create(guid))
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(x => x.ParentCommentId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                guid => guid.HasValue ? CommentId.Create(guid.Value) : null)
            .HasColumnType("uuid");

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UserName)
            .HasConversion(
                u => u.Value,
                s => UserName.Create(s).Value)
            .HasMaxLength(UserName.MAX_LENGTH)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasConversion(
                e => e.Value,
                s => Email.Create(s).Value)
            .HasMaxLength(Email.MAX_LENGTH)
            .IsRequired();

        builder.Property(x => x.HomePage)
            .HasConversion(
                h => h != null ? h.Value : null,
                s => s != null ? HomePage.Create(s).Value : null)
            .HasMaxLength(HomePage.MAX_LENGTH)
            .IsRequired(false);

        builder.Property(x => x.Text)
            .HasConversion(
                t => t.Value,
                s => Text.Create(s).Value)
            .HasMaxLength(Text.MAX_LENGTH)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Navigation(x => x.Attachments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany<CommentAttachment>(
            x => x.Attachments,
            nav =>
            {
                nav.ToTable("comment_attachments");

                nav.WithOwner().HasForeignKey("comment_id");

                nav.Property<Guid>("Id");
                nav.HasKey("Id");

                nav.Property(a => a.FileId)
                    .HasColumnName("file_id")
                    .HasColumnType("uuid")
                    .IsRequired();
            });
    }
}