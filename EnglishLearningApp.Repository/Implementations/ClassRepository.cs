using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassRoom>> GetUserClassesAsync(Guid userId)
    {
        return await _context.ClassMembers
            .Include(cm => cm.ClassRoom)
            .ThenInclude(cr => cr.Teacher)
            .Include(cm => cm.ClassRoom)
            .ThenInclude(cr => cr.Members)
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.ClassRoom)
            .Distinct()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<ClassRoom?> GetByIdAsync(Guid id, bool includeMembers = false)
    {
        var query = _context.ClassRooms
            .Include(c => c.Teacher)
            .AsQueryable();

        if (includeMembers)
        {
            query = query.Include(c => c.Members)
                         .ThenInclude(m => m.User);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<ClassRoom?> GetByCodeAsync(string code)
    {
        return await _context.ClassRooms
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.InviteCode == code);
    }

    public async Task<ClassRoom> CreateAsync(ClassRoom classRoom)
    {
        classRoom.Id = Guid.NewGuid();
        classRoom.InviteCode = GenerateInviteCode();
        classRoom.CreatedAt = DateTime.UtcNow;
        
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();
        
        // Reload with teacher info
        return (await GetByIdAsync(classRoom.Id))!;
    }

    public async Task<ClassRoom> UpdateAsync(ClassRoom classRoom)
    {
        _context.ClassRooms.Update(classRoom);
        await _context.SaveChangesAsync();
        return classRoom;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var classRoom = await _context.ClassRooms.FindAsync(id);
        if (classRoom == null) return false;

        _context.ClassRooms.Remove(classRoom);
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

        
   

   
}



