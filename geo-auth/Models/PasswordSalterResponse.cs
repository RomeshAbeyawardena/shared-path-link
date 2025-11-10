namespace geo_auth.Models;

internal record PasswordSalterResponse : StandardResponse<PasswordSalterResponse>
{
    public PasswordSalterResponse()
    {
        
    }

    public PasswordSalterResponse(Guid? automationId) : base(automationId)
    {
        
    }

    protected override PasswordSalterResponse? Result => this;
}
