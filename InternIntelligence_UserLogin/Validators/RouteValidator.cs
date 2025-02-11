namespace InternIntelligence_UserLogin.Validators
{
    public static class RouteValidator
    {
        public static RouteHandlerBuilder Validate<T>(this RouteHandlerBuilder builder, bool firstErrorOnly = true)
        {
            builder.AddEndpointFilter(async (invocationContext, next) =>
            {
                var argument = invocationContext.Arguments.OfType<T>().FirstOrDefault() ?? throw new Exception($"Filter argument of type {typeof(T).FullName} is not found");

                var response = argument.DataAnnotationsValidate();

                if (!response.IsValid)
                {
                    string? errorMessage = firstErrorOnly ?
                                            response.Results.FirstOrDefault()?.ErrorMessage :
                                            string.Join("|", response.Results.Select(x => x.ErrorMessage));

                    return Results.Problem(errorMessage, statusCode: 400);
                }

                return await next(invocationContext);
            });

            return builder;
        }
    }
}
