using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using Blogging;

using var db = new BloggingContext();


Blog sew = new Blog { BlogId = 1 };
Post post = new Post { PostId = 1, BlogId = 1, Title = "Suppa" };

post.Blog = sew; // geht nur mit Navigation Property

Console.Write(post.Blog.BlogId);

db.Database.EnsureCreated();

Console.WriteLine("--- Blog & Post System Bereit ---");
Console.WriteLine("Befehle: list, add, update, delete, add-post, list-posts, exit");

bool running = true;
while (running)
{
    Console.Write("\nBefehl > ");
    var input = Console.ReadLine()?.ToLower();

    switch (input)
    {
        case "list":
            var blogs = await db.Blogs.Include(b => b.Posts).ToListAsync();
    
            Console.WriteLine("\nBlogs in der Datenbank (inkl. Posts):");
            foreach (var b in blogs)
            {
                Console.WriteLine($"- [{b.BlogId}] {b.Url}");
                
                if (b.Posts.Any())
                {
                    foreach (var p in b.Posts)
                    {
                        Console.WriteLine($"   · Post #{p.PostId}: {p.Title}");
                    }
                }
                else
                {
                    Console.WriteLine("   (Keine Posts vorhanden)");
                }
            }

            break;

        case "add":
            Console.Write("Blog URL: ");
            db.Add(new Blog { Url = Console.ReadLine() ?? "" });
            await db.SaveChangesAsync();
            Console.WriteLine("Gespeichert.");
            break;

        case "add-post":
            Console.Write("ID des Blogs, für den der Post ist: ");
            if (int.TryParse(Console.ReadLine(), out int blogId))
            {
                var targetBlog = await db.Blogs.FindAsync(blogId);
                if (targetBlog != null)
                {
                    Console.Write("Post Titel: ");
                    var title = Console.ReadLine() ?? "Kein Titel";
                    Console.Write("Inhalt: ");
                    var content = Console.ReadLine() ?? "";

                    targetBlog.Posts.Add(new Post { Title = title, Content = content });
                    await db.SaveChangesAsync();
                    Console.WriteLine("Post hinzugefügt.");
                }
                else Console.WriteLine("Blog nicht gefunden.");
            }
            break;

        case "list-posts":
            Console.Write("ID des Blogs eingeben: ");
            if (int.TryParse(Console.ReadLine(), out int bId))
            {
                var blogWithPosts = await db.Blogs
                    .Include(b => b.Posts)
                    .FirstOrDefaultAsync(b => b.BlogId == bId);

                if (blogWithPosts != null)
                {
                    Console.WriteLine($"Posts für {blogWithPosts.Url}:");
                    foreach (var p in blogWithPosts.Posts)
                    {
                        Console.WriteLine($"  -> [{p.PostId}] {p.Title}: {p.Content}");
                    }
                }
                else Console.WriteLine("Blog nicht gefunden.");
            }
            break;

        case "update":
            Console.Write("ID des Blogs zum Ändern: ");
            if (int.TryParse(Console.ReadLine(), out int upId))
            {
                var blog = await db.Blogs.FindAsync(upId);
                if (blog != null)
                {
                    Console.Write("Neue URL: ");
                    blog.Url = Console.ReadLine() ?? blog.Url;
                    await db.SaveChangesAsync();
                }
            }
            break;

        case "exit":
            running = false;
            break;

        default:
            Console.WriteLine("Befehle: list, add, update, delete, add-post, list-posts, exit");
            break;
    }
}
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(@"Server=127.0.0.1;uid=root;pwd=insy;database=blogging");

        optionsBuilder.UseSeeding((context, _) =>
        {
            var existingBlogs = context.Set<Blog>().ToList();
            if (existingBlogs.Any())
            {
                context.Set<Blog>().RemoveRange(existingBlogs);
                context.SaveChanges();
            }

            var b1 = context.Set<Blog>();
            
            context.Set<Blog>().Add(new Blog { Url = "http://test.at" });
            context.Set<Blog>().Add(new Blog { Url = "http://test.de" });
            context.Set<Blog>().Add(new Blog { Url = "http://test.ch" });
            
            b1.Add(new Blog { Url = "http://test.net" });
            
            var comBlog = new Blog { Url = "http://test.com" };
            context.Set<Blog>().Add(comBlog);

            comBlog.Posts.Add(new Post 
            { 
                Title = "asdf", 
                Content = "Dachkatzl" 
            });
            
            context.SaveChanges();
        });
    }
}