using System.Threading.Tasks;
using Grpc.Core;
using Status;

namespace gRPCDemo
{
    class StatusServer: Status.Status.StatusBase
    {
        public override Task<StatusResponse> GetStatus(StatusRequest request, ServerCallContext context)
        {
            return base.GetStatus(request, context);
        }
    }
}