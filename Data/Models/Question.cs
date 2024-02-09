namespace LinguacApi.Data.Models
{
    public record Question(Guid Id, string Text, string CorrectAnswer, ICollection<string> IncorrectAnswers)
    {
        public IEnumerable<string> AllAnswers => IncorrectAnswers.Append(CorrectAnswer);
    }
}
