using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiSgc.Utils;

public class ApiResponse
{
    public string Message { get; set; } = "";
    public object Data { get; set; } = Array.Empty<object>();
    public int Status { get; set; } = 200;
    public bool Success { get; set; } = true;
}

public static class ApiHelper
{
    public static class Messages
    {
        public const string Ok = "Operação realizada com sucesso.";
        public const string Created = "Recurso criado com sucesso.";
        public const string NoContent = "Nenhum conteúdo encontrado.";
        public const string BadRequest = "Requisição inválida.";
        public const string Unauthorized = "Não autorizado.";
        public const string Forbidden = "Acesso proibido.";
        public const string NotFound = "Recurso não encontrado.";
        public const string InternalServerError = "Erro interno no servidor.";
        public const string DeleteSuccess = "Recurso excluído com sucesso.";
        public const string UnprocessableEntity = "Formulário inválido";
    }

    public static ApiResponse Response(
        string message = "",
        object? data = null,
        bool success = true,
        int status = 200
    )
    {
        return new ApiResponse
        {
            Success = success,
            Status = status,
            Data = data ?? Array.Empty<object>(),
            Message = message
        };
    }

    public static ApiResponse Ok(object? data = null, string message = Messages.Ok)
    {
        return Response(message, data ?? Array.Empty<object>(), true, 200);
    }

    public static ApiResponse Created(object? data = null, string message = Messages.Created)
    {
        return Response(message, data ?? Array.Empty<object>(), true, 201);
    }

    public static ApiResponse NoContent(string message = Messages.NoContent)
    {
        return Response(message, Array.Empty<object>(), true, 204);
    }

    public static ApiResponse BadRequest(string message = Messages.BadRequest)
    {
        return Response(message, Array.Empty<object>(), false, 400);
    }

    public static ApiResponse UnprocessableEntity(object? data = null, string message = Messages.UnprocessableEntity)
    {
        return Response(message, data ?? Array.Empty<object>(), false, 422);
    }

    public static ApiResponse Unauthorized(string message = Messages.Unauthorized)
    {
        return Response(message, Array.Empty<object>(), false, 401);
    }

    public static ApiResponse Forbidden(string message = Messages.Forbidden)
    {
        return Response(message, Array.Empty<object>(), false, 403);
    }

    public static ApiResponse NotFound(string message = Messages.NotFound)
    {
        return Response(message, Array.Empty<object>(), false, 404);
    }

    public static ApiResponse InternalServerError(string message = Messages.InternalServerError, object? data = null)
    {
        return Response(message, data ?? Array.Empty<object>(), false, 500);
    }

    public static ApiResponse DeleteSuccess(string message = Messages.DeleteSuccess)
    {
        return Response(message, Array.Empty<object>(), true, 200);
    }

    public static string[] GetErrorMessages(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        return errors;
    }
}
