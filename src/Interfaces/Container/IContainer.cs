namespace Nybus.Container
{
    public interface IContainer
    {
        T Resolve<T>();

        void Release<T>(T component);
    }
}