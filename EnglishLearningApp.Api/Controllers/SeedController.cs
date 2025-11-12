using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningApp.Data;
using EnglishLearningApp.Data.Entities.Chatbot;

namespace EnglishLearningApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("vocabulary")]
        public async Task<IActionResult> SeedVocabulary()
        {
            try
            {
                // Check if vocabulary already exists
                var existingVocab = await _context.Vocabularies.AnyAsync();
                if (existingVocab)
                {
                    return Ok(new { message = "Vocabulary already seeded" });
                }

                var vocabularies = new[]
                {
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Hello",
                        Meaning = "A greeting used when meeting someone",
                        Example = "Hello, how are you today?",
                        Topic = "General",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Beautiful",
                        Meaning = "Having beauty; pleasing to the senses or mind",
                        Example = "The sunset looks beautiful tonight.",
                        Topic = "General",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Important",
                        Meaning = "Of great significance or value",
                        Example = "It's important to eat healthy food.",
                        Topic = "General",
                        Level = "Intermediate"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Computer",
                        Meaning = "An electronic device for processing data",
                        Example = "I use my computer for work every day.",
                        Topic = "Technology",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Algorithm",
                        Meaning = "A set of rules for solving a problem in a finite number of steps",
                        Example = "The search algorithm helps find relevant results.",
                        Topic = "Technology",
                        Level = "Advanced"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Restaurant",
                        Meaning = "A business where people pay to sit and eat meals",
                        Example = "Let's go to that new restaurant downtown.",
                        Topic = "Food",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Delicious",
                        Meaning = "Having a very pleasant taste",
                        Example = "This pizza is absolutely delicious!",
                        Topic = "Food",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Adventure",
                        Meaning = "An unusual and exciting experience or activity",
                        Example = "Our trip to the mountains was quite an adventure.",
                        Topic = "Travel",
                        Level = "Intermediate"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Soccer",
                        Meaning = "A game played between two teams using a round ball",
                        Example = "I play soccer with my friends every weekend.",
                        Topic = "Sports",
                        Level = "Beginner"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Business",
                        Meaning = "Commercial activity or a company",
                        Example = "She started her own business last year.",
                        Topic = "Business",
                        Level = "Intermediate"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Entrepreneur",
                        Meaning = "A person who starts and runs a business",
                        Example = "The successful entrepreneur shared his story.",
                        Topic = "Business",
                        Level = "Advanced"
                    },
                    new Vocabulary
                    {
                        Id = Guid.NewGuid(),
                        Word = "Magnificent",
                        Meaning = "Extremely beautiful, elaborate, or impressive",
                        Example = "The cathedral has a magnificent architecture.",
                        Topic = "General",
                        Level = "Advanced"
                    }
                };

                _context.Vocabularies.AddRange(vocabularies);
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully seeded {vocabularies.Length} vocabulary words" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to seed vocabulary", error = ex.Message });
            }
        }
    }
}
