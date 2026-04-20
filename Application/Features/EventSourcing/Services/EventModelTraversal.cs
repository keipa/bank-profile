using System.Collections;
using BankProfiles.Web.Domain.BankProfiles;

namespace BankProfiles.Web.Application.Features.EventSourcing.Services;

internal static class EventModelTraversal
{
    public static bool IsNavigableModelType(Type type)
    {
        var candidateType = Nullable.GetUnderlyingType(type) ?? type;

        return candidateType.IsClass
            && candidateType != typeof(string)
            && !candidateType.IsGenericType
            && candidateType.Assembly == typeof(BankProfile).Assembly
            && !typeof(IEnumerable).IsAssignableFrom(candidateType);
    }
}
