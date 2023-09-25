namespace RomanaWeb.Helper
{
    public interface IRegisterScopped
    {

    }
    public interface IRegisterSingleton
    {

    }
    public static class AppRegisterServices
    {

        public static void RegisterServices<ServiceType>(IServiceCollection services)
        {
            var myServices = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());

            myServices.Where(service => typeof(ServiceType).IsAssignableFrom(service) && service != typeof(ServiceType))
                .ToList().ForEach((service) =>
                {
                    Type interfaceType = myServices.FirstOrDefault(x => x.Name == "I" + service.Name);

                    // for service
                    if (interfaceType == null && typeof(ServiceType) == typeof(IRegisterScopped))
                        services.AddScoped(service);
                    else if (interfaceType == null && typeof(ServiceType) == typeof(IRegisterSingleton))
                        services.AddSingleton(service);
                    // for interface and service
                    else if (interfaceType != null && typeof(ServiceType) == typeof(IRegisterScopped))
                        services.AddScoped(interfaceType, service);
                    else if (interfaceType != null && typeof(ServiceType) == typeof(IRegisterSingleton))
                        services.AddSingleton(interfaceType, service);
                });
        }
    }


}
