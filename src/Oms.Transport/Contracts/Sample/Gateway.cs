using System;
using System.Linq;
using System.ServiceModel;
using Oms.Transport.Contracts.Dto;

namespace Oms.Transport.Contracts.Sample
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Gateway : IGateway
    {
        public Response ExecuteRequest(Request request)
        {
            object requestHandler = null;
            try
            {
                var requestType = request.GetType();
                var responseType = GetResponseType(requestType);
                var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
                requestHandler = container.Resolve(requestHandlerType);
                return (Response)requestHandlerType.GetMethod("Handle").Invoke(requestHandler, new[] { request });
            }
            catch (BusinessLogicException bex)
            {
                var bf = new BusinessLogicFault
                {
                    Operation = requestHandler.GetType().FullName, 
                    Message = bex.Message
                };
                throw new FaultException<BusinessLogicFault>(bf);
            }
            catch (Exception ex)
            {
                var gf = new GenericFault
                {
                    Operation = requestHandler.GetType().FullName, 
                    Message = ex.Message
                };
                throw new FaultException<GenericFault>(gf);
            }
        }

        private static Type GetResponseType(Type requestType)
        {
            return GetRequestInterface(requestType).GetGenericArguments()[0];
        }

        private static Type GetRequestInterface(Type requestType)
        {
            return (
                from @interface in requestType.GetInterfaces()
                where @interface.IsGenericType
                where typeof(IRequest<>).IsAssignableFrom(
                    @interface.GetGenericTypeDefinition())
                select @interface)
                .Single();
        }
    }
}