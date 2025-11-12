using EnglishLearningApp.Data.Entities.Chatbot;
using EnglishLearningApp.Repository.Interfaces;
using EnglishLearningApp.Service.Interfaces;

namespace EnglishLearningApp.Service.Implementations;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly IUserVocabularyRepository _userVocabularyRepository;

    public VocabularyService(
        IVocabularyRepository vocabularyRepository,
        IUserVocabularyRepository userVocabularyRepository)
    {
        _vocabularyRepository = vocabularyRepository;
        _userVocabularyRepository = userVocabularyRepository;
    }

    public async Task<object> GetPaginatedAsync(int page, int pageSize, string? topic = null, string? level = null)
    {
        var (items, totalCount) = await _vocabularyRepository.GetPaginatedAsync(
            page, pageSize, null, topic, level);        return new
        {
            Items = items.Select(v => new
            {
                Id = v.Id.ToString(),
                v.Word,
                v.Meaning,
                v.Example,
                v.Topic,
                v.Level,
                v.ImageUrl
            }),
            TotalItems = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object?> GetByIdAsync(Guid id)
    {
        var vocabulary = await _vocabularyRepository.GetByIdAsync(id);
        if (vocabulary == null) return null;        return new
        {
            Id = vocabulary.Id.ToString(),
            vocabulary.Word,
            vocabulary.Meaning,
            vocabulary.Example,
            vocabulary.Topic,
            vocabulary.Level,
            vocabulary.ImageUrl
        };
    }

    public async Task<object> CreateAsync(object dto)
    {
        dynamic d = dto;
        var vocabulary = new Vocabulary
        {
            Word = d.Word,
            Meaning = d.Meaning,
            Example = d.Example,
            Topic = d.Topic,
            Level = d.Level,
            ImageUrl = d.ImageUrl
        };

        var created = await _vocabularyRepository.CreateAsync(vocabulary);
          return new
        {
            Id = created.Id.ToString(),
            created.Word,
            created.Meaning,
            created.Example,
            created.Topic,
            created.Level,
            created.ImageUrl
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _vocabularyRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<string>> GetTopicsAsync()
    {
        return await _vocabularyRepository.GetTopicsAsync();
    }

    public async Task<IEnumerable<string>> GetLevelsAsync()
    {
        return await _vocabularyRepository.GetLevelsAsync();
    }

    public async Task<object> GetUserVocabulariesAsync(Guid userId, bool? isLearned = null)
    {
        var userVocabularies = await _userVocabularyRepository.GetUserVocabulariesAsync(userId, isLearned);
        
        return userVocabularies.Select(uv => new
        {
            Id = uv.Id.ToString(),
            VocabularyId = uv.VocabularyId.ToString(),
            uv.IsLearned,
            uv.Note,
            AddedAt = uv.AddedAt,
            Vocabulary = new
            {
                Id = uv.Vocabulary.Id.ToString(),
                uv.Vocabulary.Word,
                uv.Vocabulary.Meaning,
                uv.Vocabulary.Example,
                uv.Vocabulary.Topic,
                uv.Vocabulary.Level,
                uv.Vocabulary.ImageUrl
            }
        });
    }

    public async Task<bool> ToggleLearnedAsync(Guid userId, Guid vocabularyId)
    {
        return await _userVocabularyRepository.ToggleLearnedAsync(userId, vocabularyId);
    }

    public async Task<bool> AddNoteAsync(Guid userId, Guid vocabularyId, string note)
    {
        return await _userVocabularyRepository.AddNoteAsync(userId, vocabularyId, note);
    }
}
