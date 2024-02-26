namespace LinguacApi.Data.Dtos
{
    public record QuestionDto(
        Guid Id,
        string Text,
        IEnumerable<AnswerDto> Answers
    );
}
