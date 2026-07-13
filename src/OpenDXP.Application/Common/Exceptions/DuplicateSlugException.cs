namespace OpenDXP.Application.Common.Exceptions;

public class DuplicateSlugException : Exception
{
    public DuplicateSlugException(string slug)
        : base($"A page with slug '{slug}' already exists.")
    {
    }
}
