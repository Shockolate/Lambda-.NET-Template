namespace RestfulMicroserverless.Contracts
{
    public interface IPayloadConverter
    {
        string ConvertToPayload(object objectToConvert);
        T ConvertFromPayload<T>(string payload);
    }
}