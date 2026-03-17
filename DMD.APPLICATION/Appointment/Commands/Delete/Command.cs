using DMD.APPLICATION.Responses;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.Appointment.Commands.Delete
{
    [JsonSchema("AppointmentDeleteCommand")]
    public class Command : IRequest<Response>
    {
        public int Id { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly DmdDbContext dbContext;

        public CommandHandler(DmdDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var item = await dbContext.AppointmentRequests
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (item == null)
                    return new BadRequestResponse("Item may have been modified or removed.");

                dbContext.AppointmentRequests.Remove(item);
                await dbContext.SaveChangesAsync(cancellationToken);

                return new SuccessResponse<bool>(true);
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }
    }
}
