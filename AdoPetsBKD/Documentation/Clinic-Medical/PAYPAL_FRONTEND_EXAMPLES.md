# Ejemplos de Integración Frontend - PayPal

## ?? React/TypeScript - Componente de Pago con PayPal

### 1. Servicio de Pagos (pagoService.ts)

```typescript
// src/services/pagoService.ts
import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

export interface CreatePayPalOrderRequest {
  monto: number;
  concepto: string;
  esAnticipo: boolean;
  montoTotal?: number;
  citaId?: string;
  solicitudCitaId?: string;
  returnUrl: string;
  cancelUrl: string;
}

export interface PayPalOrderResponse {
  orderId: string;
  approvalUrl: string;
  status: string;
}

export interface CapturePaymentRequest {
  orderId: string;
}

export interface PagoDto {
  id: string;
  numeroPago: string;
  monto: number;
  metodoPago: string;
  estado: string;
  payPalOrderId?: string;
  payPalCaptureId?: string;
  payPalPayerEmail?: string;
  payPalPayerName?: string;
  concepto?: string;
  esAnticipo: boolean;
  montoTotal?: number;
  montoRestante?: number;
  createdAt: string;
}

export class PagoService {
  private getAuthHeaders() {
    const token = localStorage.getItem('auth_token');
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  /**
   * Crea una orden de pago en PayPal
   */
  async createPayPalOrder(request: CreatePayPalOrderRequest): Promise<PayPalOrderResponse> {
    const response = await axios.post(
      `${API_BASE_URL}/api/pagos/paypal/create-order`,
      request,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  /**
   * Captura un pago después de que el usuario lo apruebe
   */
  async capturePayment(request: CapturePaymentRequest): Promise<PagoDto> {
    const response = await axios.post(
      `${API_BASE_URL}/api/pagos/paypal/capture`,
      request,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  /**
   * Obtiene un pago por su ID
   */
  async getPagoById(pagoId: string): Promise<PagoDto> {
    const response = await axios.get(
      `${API_BASE_URL}/api/pagos/${pagoId}`,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  /**
   * Obtiene el historial de pagos del usuario
   */
  async getHistorialPagos(usuarioId: string): Promise<PagoDto[]> {
    const response = await axios.get(
      `${API_BASE_URL}/api/pagos/usuario/${usuarioId}`,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }
}

export const pagoService = new PagoService();
```

---

### 2. Hook personalizado (usePayPal.ts)

```typescript
// src/hooks/usePayPal.ts
import { useState } from 'react';
import { pagoService, CreatePayPalOrderRequest, PagoDto } from '../services/pagoService';
import { useNavigate } from 'react-router-dom';

export const usePayPal = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  /**
   * Inicia el proceso de pago con PayPal
   */
  const iniciarPagoPayPal = async (
    monto: number,
    concepto: string,
    esAnticipo: boolean = false,
    montoTotal?: number,
    citaId?: string
  ) => {
    try {
      setLoading(true);
      setError(null);

      const currentUrl = window.location.origin;
      const request: CreatePayPalOrderRequest = {
        monto,
        concepto,
        esAnticipo,
        montoTotal,
        citaId,
        returnUrl: `${currentUrl}/payment/success`,
        cancelUrl: `${currentUrl}/payment/cancel`
      };

      const orden = await pagoService.createPayPalOrder(request);

      // Guardar el orderId en localStorage para recuperarlo después
      localStorage.setItem('paypal_order_id', orden.orderId);
      localStorage.setItem('paypal_payment_info', JSON.stringify({
        monto,
        concepto,
        esAnticipo,
        citaId
      }));

      // Redirigir al usuario a PayPal
      window.location.href = orden.approvalUrl;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al crear orden de PayPal');
      setLoading(false);
    }
  };

  /**
   * Captura el pago después de la aprobación
   */
  const capturarPago = async (): Promise<PagoDto | null> => {
    try {
      setLoading(true);
      setError(null);

      const orderId = localStorage.getItem('paypal_order_id');
      if (!orderId) {
        throw new Error('No se encontró el ID de la orden');
      }

      const pago = await pagoService.capturePayment({ orderId });

      // Limpiar localStorage
      localStorage.removeItem('paypal_order_id');
      localStorage.removeItem('paypal_payment_info');

      setLoading(false);
      return pago;
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al capturar el pago');
      setLoading(false);
      return null;
    }
  };

  /**
   * Cancela el proceso de pago
   */
  const cancelarPago = () => {
    localStorage.removeItem('paypal_order_id');
    localStorage.removeItem('paypal_payment_info');
    navigate('/');
  };

  return {
    iniciarPagoPayPal,
    capturarPago,
    cancelarPago,
    loading,
    error
  };
};
```

---

### 3. Componente de Checkout (PaymentCheckout.tsx)

```typescript
// src/components/PaymentCheckout.tsx
import React, { useState } from 'react';
import { usePayPal } from '../hooks/usePayPal';
import { Alert, Button, Card, Form, Spinner } from 'react-bootstrap';

interface PaymentCheckoutProps {
  monto: number;
  concepto: string;
  citaId?: string;
  onSuccess?: () => void;
  onCancel?: () => void;
}

export const PaymentCheckout: React.FC<PaymentCheckoutProps> = ({
  monto,
  concepto,
  citaId,
  onSuccess,
  onCancel
}) => {
  const { iniciarPagoPayPal, loading, error } = usePayPal();
  const [metodoPago, setMetodoPago] = useState<'completo' | 'anticipo'>('completo');
  const [montoAPagar, setMontoAPagar] = useState(monto);

  const handleMetodoPagoChange = (metodo: 'completo' | 'anticipo') => {
    setMetodoPago(metodo);
    setMontoAPagar(metodo === 'anticipo' ? monto * 0.5 : monto);
  };

  const handlePagar = async () => {
    await iniciarPagoPayPal(
      montoAPagar,
      concepto,
      metodoPago === 'anticipo',
      metodoPago === 'anticipo' ? monto : undefined,
      citaId
    );
  };

  return (
    <Card className="payment-checkout">
      <Card.Header>
        <h5>?? Realizar Pago</h5>
      </Card.Header>
      <Card.Body>
        {error && <Alert variant="danger">{error}</Alert>}

        <div className="mb-3">
          <h6>Concepto:</h6>
          <p>{concepto}</p>
        </div>

        <Form.Group className="mb-3">
          <Form.Label>Método de Pago</Form.Label>
          <div>
            <Form.Check
              type="radio"
              id="pago-completo"
              label={`Pago Completo - $${monto.toFixed(2)} MXN`}
              checked={metodoPago === 'completo'}
              onChange={() => handleMetodoPagoChange('completo')}
            />
            <Form.Check
              type="radio"
              id="pago-anticipo"
              label={`Anticipo 50% - $${(monto * 0.5).toFixed(2)} MXN`}
              checked={metodoPago === 'anticipo'}
              onChange={() => handleMetodoPagoChange('anticipo')}
            />
          </div>
          {metodoPago === 'anticipo' && (
            <Form.Text className="text-muted">
              Saldo restante: ${(monto * 0.5).toFixed(2)} MXN
            </Form.Text>
          )}
        </Form.Group>

        <div className="d-flex gap-2">
          <Button
            variant="primary"
            onClick={handlePagar}
            disabled={loading}
            className="flex-grow-1"
          >
            {loading ? (
              <>
                <Spinner animation="border" size="sm" className="me-2" />
                Procesando...
              </>
            ) : (
              <>
                <i className="fab fa-paypal me-2"></i>
                Pagar con PayPal
              </>
            )}
          </Button>
          <Button
            variant="outline-secondary"
            onClick={onCancel}
            disabled={loading}
          >
            Cancelar
          </Button>
        </div>
      </Card.Body>
    </Card>
  );
};
```

---

### 4. Página de Éxito (PaymentSuccess.tsx)

```typescript
// src/pages/PaymentSuccess.tsx
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { usePayPal } from '../hooks/usePayPal';
import { PagoDto } from '../services/pagoService';
import { Alert, Button, Card, Spinner } from 'react-bootstrap';

export const PaymentSuccess: React.FC = () => {
  const { capturarPago, loading, error } = usePayPal();
  const [pago, setPago] = useState<PagoDto | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const procesarPago = async () => {
      const pagoCapturado = await capturarPago();
      if (pagoCapturado) {
        setPago(pagoCapturado);
      }
    };

    procesarPago();
  }, []);

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '400px' }}>
        <div className="text-center">
          <Spinner animation="border" variant="primary" />
          <p className="mt-3">Procesando tu pago...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-5">
        <Alert variant="danger">
          <Alert.Heading>? Error al Procesar el Pago</Alert.Heading>
          <p>{error}</p>
          <hr />
          <div className="d-flex gap-2">
            <Button variant="outline-danger" onClick={() => navigate('/')}>
              Volver al Inicio
            </Button>
          </div>
        </Alert>
      </div>
    );
  }

  if (!pago) {
    return (
      <div className="container mt-5">
        <Alert variant="warning">
          No se encontró información del pago.
        </Alert>
      </div>
    );
  }

  return (
    <div className="container mt-5">
      <Card className="text-center">
        <Card.Body>
          <div className="mb-4">
            <i className="fas fa-check-circle text-success" style={{ fontSize: '4rem' }}></i>
          </div>
          <h2 className="text-success mb-3">¡Pago Exitoso!</h2>
          <p className="text-muted">Tu pago ha sido procesado correctamente</p>

          <Card className="mt-4 text-start">
            <Card.Header>
              <strong>Detalles del Pago</strong>
            </Card.Header>
            <Card.Body>
              <div className="row mb-2">
                <div className="col-6"><strong>Folio:</strong></div>
                <div className="col-6">{pago.numeroPago}</div>
              </div>
              <div className="row mb-2">
                <div className="col-6"><strong>Monto:</strong></div>
                <div className="col-6">${pago.monto.toFixed(2)} {pago.moneda}</div>
              </div>
              <div className="row mb-2">
                <div className="col-6"><strong>Método:</strong></div>
                <div className="col-6">{pago.metodoPago}</div>
              </div>
              <div className="row mb-2">
                <div className="col-6"><strong>Estado:</strong></div>
                <div className="col-6">
                  <span className="badge bg-success">{pago.estado}</span>
                </div>
              </div>
              {pago.concepto && (
                <div className="row mb-2">
                  <div className="col-6"><strong>Concepto:</strong></div>
                  <div className="col-6">{pago.concepto}</div>
                </div>
              )}
              {pago.esAnticipo && (
                <div className="row mb-2">
                  <div className="col-6"><strong>Saldo Restante:</strong></div>
                  <div className="col-6 text-warning">
                    ${pago.montoRestante?.toFixed(2)} {pago.moneda}
                  </div>
                </div>
              )}
              <div className="row">
                <div className="col-6"><strong>Fecha:</strong></div>
                <div className="col-6">
                  {new Date(pago.createdAt).toLocaleString('es-MX')}
                </div>
              </div>
            </Card.Body>
          </Card>

          <div className="d-flex gap-2 justify-content-center mt-4">
            <Button variant="primary" onClick={() => navigate('/mis-pagos')}>
              Ver Mis Pagos
            </Button>
            <Button variant="outline-primary" onClick={() => navigate('/')}>
              Volver al Inicio
            </Button>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
};
```

---

### 5. Página de Cancelación (PaymentCancel.tsx)

```typescript
// src/pages/PaymentCancel.tsx
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { usePayPal } from '../hooks/usePayPal';
import { Alert, Button, Card } from 'react-bootstrap';

export const PaymentCancel: React.FC = () => {
  const { cancelarPago } = usePayPal();
  const navigate = useNavigate();
  const paymentInfo = JSON.parse(localStorage.getItem('paypal_payment_info') || '{}');

  const handleVolverIntentar = () => {
    navigate(-1); // Volver a la página anterior
  };

  const handleCancelar = () => {
    cancelarPago();
    navigate('/');
  };

  return (
    <div className="container mt-5">
      <Card className="text-center">
        <Card.Body>
          <div className="mb-4">
            <i className="fas fa-times-circle text-warning" style={{ fontSize: '4rem' }}></i>
          </div>
          <h2 className="text-warning mb-3">Pago Cancelado</h2>
          <p>Has cancelado el proceso de pago con PayPal.</p>

          {paymentInfo.concepto && (
            <Alert variant="info" className="mt-4">
              <strong>Información del pago:</strong>
              <br />
              {paymentInfo.concepto}
              <br />
              Monto: ${paymentInfo.monto?.toFixed(2)} MXN
            </Alert>
          )}

          <div className="d-flex gap-2 justify-content-center mt-4">
            <Button variant="primary" onClick={handleVolverIntentar}>
              <i className="fas fa-redo me-2"></i>
              Intentar de Nuevo
            </Button>
            <Button variant="outline-secondary" onClick={handleCancelar}>
              Cancelar Definitivamente
            </Button>
          </div>
        </Card.Body>
      </Card>
    </div>
  );
};
```

---

### 6. Componente de Historial de Pagos (PaymentHistory.tsx)

```typescript
// src/components/PaymentHistory.tsx
import React, { useEffect, useState } from 'react';
import { pagoService, PagoDto } from '../services/pagoService';
import { Badge, Card, Spinner, Table } from 'react-bootstrap';

interface PaymentHistoryProps {
  usuarioId: string;
}

export const PaymentHistory: React.FC<PaymentHistoryProps> = ({ usuarioId }) => {
  const [pagos, setPagos] = useState<PagoDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const cargarPagos = async () => {
      try {
        const data = await pagoService.getHistorialPagos(usuarioId);
        setPagos(data);
      } catch (error) {
        console.error('Error al cargar historial de pagos:', error);
      } finally {
        setLoading(false);
      }
    };

    cargarPagos();
  }, [usuarioId]);

  const getEstadoBadge = (estado: string) => {
    const badges: Record<string, string> = {
      'Completado': 'success',
      'Pendiente': 'warning',
      'Fallido': 'danger',
      'Cancelado': 'secondary'
    };
    return badges[estado] || 'info';
  };

  if (loading) {
    return (
      <div className="text-center p-4">
        <Spinner animation="border" />
      </div>
    );
  }

  if (pagos.length === 0) {
    return (
      <Card>
        <Card.Body className="text-center text-muted">
          <i className="fas fa-receipt fa-3x mb-3"></i>
          <p>No tienes pagos registrados</p>
        </Card.Body>
      </Card>
    );
  }

  return (
    <Card>
      <Card.Header>
        <h5><i className="fas fa-history me-2"></i>Historial de Pagos</h5>
      </Card.Header>
      <Card.Body className="p-0">
        <Table striped hover responsive>
          <thead>
            <tr>
              <th>Folio</th>
              <th>Fecha</th>
              <th>Concepto</th>
              <th>Monto</th>
              <th>Método</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            {pagos.map((pago) => (
              <tr key={pago.id}>
                <td>
                  <small className="text-muted">{pago.numeroPago}</small>
                </td>
                <td>
                  {new Date(pago.createdAt).toLocaleDateString('es-MX')}
                </td>
                <td>
                  {pago.concepto}
                  {pago.esAnticipo && (
                    <Badge bg="warning" className="ms-2">Anticipo</Badge>
                  )}
                </td>
                <td>
                  <strong>${pago.monto.toFixed(2)}</strong>
                  {pago.esAnticipo && pago.montoRestante && (
                    <div className="small text-muted">
                      Saldo: ${pago.montoRestante.toFixed(2)}
                    </div>
                  )}
                </td>
                <td>{pago.metodoPago}</td>
                <td>
                  <Badge bg={getEstadoBadge(pago.estado)}>
                    {pago.estado}
                  </Badge>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </Card.Body>
    </Card>
  );
};
```

---

### 7. Configuración de Rutas (App.tsx)

```typescript
// src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { PaymentSuccess } from './pages/PaymentSuccess';
import { PaymentCancel } from './pages/PaymentCancel';
// ... otros imports

function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Otras rutas */}
        <Route path="/payment/success" element={<PaymentSuccess />} />
        <Route path="/payment/cancel" element={<PaymentCancel />} />
        {/* ... */}
      </Routes>
    </BrowserRouter>
  );
}

export default App;
```

---

## ?? Notas de Implementación

### Variables de Entorno
Crea un archivo `.env` en tu proyecto frontend:

```env
REACT_APP_API_URL=http://localhost:5000
```

### Dependencias Necesarias
```bash
npm install axios react-router-dom bootstrap react-bootstrap
npm install @fortawesome/fontawesome-free  # Para los iconos
```

### Estilos CSS Adicionales
```css
/* src/styles/payment.css */
.payment-checkout {
  max-width: 500px;
  margin: 0 auto;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.payment-success-icon {
  animation: scaleIn 0.5s ease-in-out;
}

@keyframes scaleIn {
  from {
    transform: scale(0);
  }
  to {
    transform: scale(1);
  }
}
```

---

## ?? Flujo Completo Implementado

1. Usuario hace clic en "Pagar"
2. `PaymentCheckout` muestra opciones de pago
3. Usuario selecciona método y hace clic en "Pagar con PayPal"
4. `usePayPal.iniciarPagoPayPal()` crea la orden
5. Usuario es redirigido a PayPal
6. Usuario aprueba el pago en PayPal
7. PayPal redirige a `/payment/success`
8. `PaymentSuccess` captura el pago automáticamente
9. Muestra confirmación con detalles del pago

---

**Desarrollado por**: Developer 3 - Beto  
**Fecha**: Enero 2024  
**Stack**: React + TypeScript + Bootstrap
