namespace SubastaYa.Api.Services;

// Se traduce a 404 Not Found
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
}

// Se traduce a 400 Bad Request (validaciones de negocio: subasta no activa, monto inválido, etc.)
public class SolicitudInvalidaException : Exception
{
    public SolicitudInvalidaException(string mensaje) : base(mensaje) { }
}

// Se traduce a 422 Unprocessable Entity (saldo insuficiente)
public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException(string mensaje) : base(mensaje) { }
}

// Se traduce a 409 Conflict (choque de concurrencia optimista o versión desactualizada)
public class ConflictoConcurrenciaException : Exception
{
    public ConflictoConcurrenciaException(string mensaje) : base(mensaje) { }
}
