using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Infrastructure.Data;
using ProductPriceTracker.Infrastructure.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ScrapeDbContext _context;

    public UserRepository(ScrapeDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.FindAsync(id);

    public async Task AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SaveChangesAsync() =>
        (await _context.SaveChangesAsync()) > 0;

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.Where(x => x.Username == username).FirstOrDefaultAsync();
    }

    public void Update(User obj)
    {
        _context.Users.Update(obj);
    }

    public bool IsExists(string includeProperties, string value)
    {
        if (includeProperties == "Username")
        {
            return _context.Users.Any(u => u.Username == value);
        }
        else
        {
            return false;
        }
    }
}