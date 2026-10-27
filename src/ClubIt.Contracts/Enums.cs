namespace ClubIt.Contracts;

// Mirrors of the domain enums so the mobile app does not depend on the domain layer.
public enum ClubType
{
    CreditCard = 1,
    ConsumerClub = 2,
    WorkersCommittee = 3
}

public enum BenefitType
{
    Percentage = 1,
    FixedAmount = 2,
    Coupon = 3,
    DiscountedTicket = 4
}
