using EnglishLearningApp.Data.Entities.Class;
using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Service.Implementations;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IClassMemberRepository _classMemberRepository;

    public ClassService(
        IClassRepository classRepository,
        IClassMemberRepository classMemberRepository)
    {
        _classRepository = classRepository;
        _classMemberRepository = classMemberRepository;
    }

    public async Task<object> GetUserClassesAsync(Guid userId)
    {
        var classes = await _classRepository.GetUserClassesAsync(userId);
        
        return classes.Select(c => new
        {
            Id = c.Id.ToString(),
            c.Name,
            c.Description,
            Code = c.InviteCode,
            CreatedBy = c.TeacherId.ToString(),
            CreatedByName = c.Teacher.FullName,
            CreatedAt = c.CreatedAt,
            MemberCount = c.Members.Count
        });
    }

    public async Task<object?> GetByIdAsync(Guid id)
    {
        var classRoom = await _classRepository.GetByIdAsync(id, includeMembers: true);
        if (classRoom == null) return null;

        return new
        {
            Id = classRoom.Id.ToString(),
            classRoom.Name,
            classRoom.Description,
            Code = classRoom.InviteCode,
            CreatedBy = classRoom.TeacherId.ToString(),
            CreatedByName = classRoom.Teacher.FullName,
            CreatedAt = classRoom.CreatedAt,
            MemberCount = classRoom.Members.Count,
            Members = classRoom.Members.Select(m => new
            {
                Id = m.Id.ToString(),
                UserId = m.UserId.ToString(),
                UserName = m.User.FullName,
                UserEmail = m.User.Email,
                m.Role,
                JoinedAt = m.JoinedAt
            })
        };
    }

    public async Task<object> CreateAsync(Guid userId, string name, string? description)
    {
        var classRoom = new ClassRoom
        {
            Name = name,
            Description = description ?? "",
            TeacherId = userId
        };

        var created = await _classRepository.CreateAsync(classRoom);

        // Add creator as member
        var member = new ClassMember
        {
            ClassRoomId = created.Id,
            UserId = userId,
            Role = "Teacher"
        };
        await _classMemberRepository.AddMemberAsync(member);

        // Reload to get full data
        var result = await _classRepository.GetByIdAsync(created.Id);
        
        return new
        {
            Id = result!.Id.ToString(),
            result.Name,
            result.Description,
            Code = result.InviteCode,
            CreatedBy = result.TeacherId.ToString(),
            CreatedByName = result.Teacher.FullName,
            CreatedAt = result.CreatedAt,
            MemberCount = 1
        };
    }

    public async Task<object?> JoinClassAsync(Guid userId, string code)
    {
        var classRoom = await _classRepository.GetByCodeAsync(code);
        if (classRoom == null)
        {
            throw new InvalidOperationException("Class not found");
        }

        // Check if already a member
        var isMember = await _classMemberRepository.IsMemberAsync(classRoom.Id, userId);
        if (isMember)
        {
            throw new InvalidOperationException("Already a member of this class");
        }

        // Add as member
        var member = new ClassMember
        {
            ClassRoomId = classRoom.Id,
            UserId = userId,
            Role = "Student"
        };
        await _classMemberRepository.AddMemberAsync(member);

        return new
        {
            Id = classRoom.Id.ToString(),
            classRoom.Name,
            classRoom.Description,
            Code = classRoom.InviteCode,
            CreatedBy = classRoom.TeacherId.ToString(),
            CreatedByName = classRoom.Teacher.FullName,
            CreatedAt = classRoom.CreatedAt
        };
    }

    public async Task<bool> LeaveClassAsync(Guid userId, Guid classId)
    {
        return await _classMemberRepository.RemoveMemberAsync(classId, userId);
    }

    public async Task<object> GetMembersAsync(Guid classId)
    {
        var members = await _classMemberRepository.GetClassMembersAsync(classId);
        
        return members.Select(m => new
        {
            Id = m.Id.ToString(),
            UserId = m.UserId.ToString(),
            UserName = m.User.FullName,
            UserEmail = m.User.Email,
            m.Role,
            JoinedAt = m.JoinedAt
        });
    }

    // Quiz methods - placeholder for now
    public Task<object> GetClassQuizzesAsync(Guid classId)
    {
        throw new NotImplementedException("Quiz features will be implemented later");
    }

    public Task<object?> GetQuizAsync(Guid id)
    {
        throw new NotImplementedException("Quiz features will be implemented later");
    }

    public Task<object> CreateQuizAsync(Guid userId, Guid classId, object dto)
    {
        throw new NotImplementedException("Quiz features will be implemented later");
    }

    public Task<object> SubmitQuizAsync(Guid userId, Guid quizId, object submission)
    {
        throw new NotImplementedException("Quiz features will be implemented later");
    }    public Task<object> GetQuizResultsAsync(Guid quizId)
    {
        throw new NotImplementedException("Quiz features will be implemented later");
    }
}
