namespace TswgCsTpl;

public interface IMk<T>
	where T : class
{
	static abstract T Mk();
}
