namespace MoneroBot.Daemon.Features;

using Microsoft.Extensions.Logging;
using MoneroBot.WalletRpc;
using MoneroBot.WalletRpc.Models;
using MoneroBot.WalletRpc.Models.Generated;

public record GetAddressIndex(string Address);

public record AddressIndex(uint Major, uint Minor);

public interface IGetAddressIndexHander
{
    public Task<AddressIndex?> HandleAsync(GetAddressIndex request, CancellationToken token = default);
}

public class GetAddressIndexHandler : IGetAddressIndexHander
{
    private readonly ILogger<GetAddressIndexHandler> logger;
    private readonly IWalletRpcClient wallet;

    public GetAddressIndexHandler(ILogger<GetAddressIndexHandler> logger, IWalletRpcClient wallet)
    {
        this.logger = logger;
        this.wallet = wallet;
    }

    public async Task<AddressIndex?> HandleAsync(GetAddressIndex request, CancellationToken token = default)
    {
        try
        {
            var response = await this.wallet.JsonRpcAsync<GetAddressIndexResult>(
                new MoneroRpcRequest("get_address_index", new GetAddressIndexParameters(request.Address)),
                token);
            if (response.Result is { Index: { Major: { } major, Minor: { } minor } })
            {
                return new(major, minor);
            }

            if (response.Error is { } error)
            {
                this.logger.LogError(
                    "Failed to retrieve the address index of {address}: {@error}",
                    request.Address,
                    error);
            }
            else
            {
                this.logger.LogError(
                    "Failed to retrieve rhe address index of {address} - the RPC returned a response but it was empty",
                    request.Address);
            }
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "An unhandled exception occured whilst trying to retrieve the address index of {address}: {@exception}",
                request.Address,
                exception);
        }

        return null;
    }
}
