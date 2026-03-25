using Fulfilment.Application.DTOs;
using SharedKernel;

namespace Fulfilment.Application.Queries.GetShipmentByOrder;

public interface IGetShipmentByOrderQueryHandler
    : IQueryHandler<GetShipmentByOrderQuery, ShipmentSummaryResponse?> { }
