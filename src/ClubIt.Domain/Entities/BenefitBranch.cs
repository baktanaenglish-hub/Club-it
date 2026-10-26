namespace ClubIt.Domain.Entities;

public class BenefitBranch
{
    public int BenefitId { get; set; }
    public Benefit? Benefit { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
}
