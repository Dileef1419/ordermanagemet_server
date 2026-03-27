# Order Management Server

A robust, microservices-based order management platform built with .NET 8/9 C#. This architecture demonstrates advanced patterns including **CQRS (Command-Query Responsibility Segregation)**, Clean Architecture, and Idempotency.

## 🏗 System Architecture

The project is composed of several independent microservices, all routed through a central API Gateway.

### Core Services:
* **ApiGateway** (`http://localhost:5000`): The entry point for all client requests. It acts as a reverse proxy, routing traffic to the appropriate backend microservices.
* **Auth.Api** (`http://localhost:5010`): Handles user registration, authentication, and JWT token issuance.
* **Catalog.Api** (`http://localhost:5020`): Manages the product catalog.
* **Orders.Api** (`http://localhost:5030`): Manages the lifecycle of an order (Placement, Cancellation) using EF Core for writes and Dapper for high-performance reads.
* **Payments.Api** (`http://localhost:5040`): Handles payment flows including authorization, capture, and refunds.
* **Fulfilment.Api** (`http://localhost:5050`): Manages shipments, dispatching, and warehouse queues.

### Key Architectural Patterns & Features
* **CQRS Pattern**: Write operations (Commands) use **Entity Framework Core**, while Read operations (Queries) use **Dapper** for optimized performance.
* **Idempotency**: Critical transactional endpoints (like Place Order, Register User, Authorize Payment) require an `Idempotency-Key` header (GUID) to prevent duplicate processing.
* **FluentValidation**: Request models are robustly validated.
* **Global Exception Handling**: Centralized error mapping to standardize API responses (`ValidationProblemDetails`).

---

## 🚀 API Endpoints

All external traffic should ideally route through the **ApiGateway** on `http://localhost:5000`. The paths below reflect the routes available through the gateway.

### 🛡 Auth API (`/api/v1/auth`)

| Method | Endpoint | Description | Headers / Auth |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Register a new user. | `Idempotency-Key: <guid>` |
| `POST` | `/api/v1/auth/login` | Login and receive a JWT token. | None |

---

### 📦 Catalog API (`/api/v1/catalog` or `/api/Catalog`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/Catalog` | Verify the Catalog API is operational. |
| `GET` | `/health` | Service health check. |

---

### 🛒 Orders API (`/api/v1/orders`)

| Method | Endpoint | Description | Headers / Auth |
|---|---|---|---|
| `POST` | `/api/v1/orders` | Place a new order. | `Idempotency-Key: <guid>` |
| `PUT` | `/api/v1/orders/{orderId}/cancel`| Cancel an existing order. | None |
| `GET` | `/api/v1/orders/{orderId}` | Get order details by ID. | None |
| `GET` | `/api/v1/orders/by-customer/{customerId}` | Get orders by customer ID. Supports pagination (`page`, `pageSize`) and `status` filtering. | None |
| `GET` | `/api/v1/orders/dashboard` | Get order dashboard analytics (aggregate counts by status). | None |

---

### 💳 Payments API (`/api/v1/payments`)

| Method | Endpoint | Description | Headers / Auth |
|---|---|---|---|
| `POST` | `/api/v1/payments/authorise` | Authorise a payment. | `Idempotency-Key: <guid>` |
| `POST` | `/api/v1/payments/{paymentId}/capture` | Capture a previously authorised payment.| None |
| `POST` | `/api/v1/payments/{paymentId}/refund` | Refund a captured payment. | None |
| `GET` | `/api/v1/payments/by-order/{orderId}`| Get payment details associated with an order.| None |
| `GET` | `/api/v1/payments/revenue` | Get daily revenue report. Query params: `from`, `to`, `currency`. | None |

---

### 🚚 Fulfilment API (`/api/v1/fulfilment`)

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/fulfilment/shipments` | Create a new shipment for an order. |
| `PUT` | `/api/v1/fulfilment/shipments/{shipmentId}/dispatch`| Dispatch a shipment with carrier tracking details. |
| `GET` | `/api/v1/fulfilment/by-order/{orderId}` | Get shipment details by order ID. |
| `GET` | `/api/v1/fulfilment/warehouse/{warehouseId}/queue` | Get warehouse fulfilment queue. Supports pagination and `status` filtering. |

---

## 🛠 Running the Project

1. **Open the Solution**: Load `src/OrderPlatform.slnx` in your preferred IDE (Visual Studio, JetBrains Rider, or VS Code).
2. **Start the Solution**:
   Ensure all `.Api` projects and the `ApiGateway` are set to run simultaneously, or use a tool like .NET Aspire / Tye if configured.
3. **Swagger UI**:
   Each service has its own Swagger endpoint (e.g., `http://localhost:5010/swagger`) which can be used to interactively explore and test the endpoints directly if running in `Development` mode.

