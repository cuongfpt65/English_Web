namespace EnglishLearningApp.Api.DTOs;

public class VocabularyDto
{
    public string Id { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? Topic { get; set; }
    public string? Level { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVocabularyDto
{
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? Topic { get; set; }
    public string? Level { get; set; }
    public string? ImageUrl { get; set; }
}

public class UserVocabularyDto
{
    public string Id { get; set; } = string.Empty;
    public VocabularyDto Vocabulary { get; set; } = new();
    public bool IsLearned { get; set; }
    public string? Note { get; set; }
    public DateTime LearnedAt { get; set; }
}

public class AddNoteDto
{
    public string Note { get; set; } = string.Empty;
}

public class VocabularyQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Topic { get; set; }
    public string? Level { get; set; }
}

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AddNoteRequestDto
{
    public string Note { get; set; } = string.Empty;
}

public class CreateVocabularyRequestDto
{
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? Topic { get; set; }
    public string? Level { get; set; }
    public string? ImageUrl { get; set; }
}
