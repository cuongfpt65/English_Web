using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Repository.Implementations;

public class ClassMemberRepository : IClassMemberRepository
{
    private readonly AppDbContext _context;

    public ClassMemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClassMember>> GetClassMembersAsync(Guid classId)
    {
        return await _context.ClassMembers
            .Include(cm => cm.User)
            .Where(cm => cm.ClassRoomId == classId)
            .ToListAsync();
    }

    public async Task<ClassMember?> GetMemberAsync(Guid classId, Guid userId)
    {
        return await _context.ClassMembers
            .FirstOrDefaultAsync(cm => cm.ClassRoomId == classId && cm.UserId == userId);
    }

    public async Task<ClassMember> AddMemberAsync(ClassMember member)
    {
        member.Id = Guid.NewGuid();
        member.JoinedAt = DateTime.UtcNow;
        _context.ClassMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<bool> RemoveMemberAsync(Guid classId, Guid userId)
    {
        var member = await GetMemberAsync(classId, userId);
        if (member == null) return false;

        _context.ClassMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsMemberAsync(Guid classId, Guid userId)
    {
        return await _context.ClassMembers
            .AnyAsync(cm => cm.ClassRoomId == classId && cm.UserId == userId);
    }
}
