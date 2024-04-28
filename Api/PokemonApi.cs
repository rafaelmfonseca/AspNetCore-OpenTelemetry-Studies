using System.Diagnostics;

namespace AspNetCore_OpenTelemetry_Studies.Api;

public static class PokemonApi
{
    public static void MapPokemonApiEndpoints(this IEndpointRouteBuilder endpoints, WebApplication webApp)
    {
        var group = endpoints.MapGroup("Pokemon");

        var pokemons = new[]
        {
            "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard", "Squirtle", "Wartortle", "Blastoise", "Caterpie",
        };

        const string PokemonCount = "pokemon.count";

        group.MapGet("", () =>
        {
            var items = Enumerable.Range(1, 5).Select(i => new Pokemon(pokemons[i], i)).ToArray();

            webApp.Logger.LogInformation("Returning pokemons size: {count}", items.Count());

            return items;
        }).WithName("GetPokemons").WithOpenApi();

        group.MapGet("WithNestedActivity", () =>
        {
            Pokemon[] GetRandomPokemons(int size)
            {
                using var childActivity = Telemetry.ActivitySource.StartActivity("PokemonApi/WithActivity/GetRandomPokemons");

                return Enumerable.Range(1, size).Select(i => new Pokemon(pokemons[i], i)).ToArray();
            }

            using var activity = Telemetry.ActivitySource.StartActivity("PokemonApi/WithActivity");

            var items = GetRandomPokemons(Random.Shared.Next(1, 10));

            webApp.Logger.LogInformation("Returning pokemons size: {count}", items.Count());

            return items;
        }).WithName("GetPokemonsWithNestedActivity").WithOpenApi();

        group.MapGet("WithTags", () =>
        {
            using var activity = Telemetry.ActivitySource.StartActivity("PokemonApi/WithTags");
            // attach key/value pairs to an Activity so it carries more information about the
            // current operation that it’s tracking
            // all Tag names shoulde be defined in constants rather than defined inline as this
            // provides both consistency and also discoverability
            activity?.SetTag(PokemonCount, pokemons.Length);

            var items = pokemons.Select((pokemon, index) => new Pokemon(pokemon, index)).ToArray();

            // event is a human-readable message on an Activity that represents “something happening”
            // during its lifetime.
            // events can also be created with a timestamp and a collection of Tags.
            activity?.AddEvent(new ActivityEvent("Returning pokemons"));

            // nested activies in the same scope
            // NOT RECOMMENDED BY OPENTELEMETRY!
            using (var nestedActivity = Telemetry.ActivitySource.StartActivity("PokemonApi/WithTags/NestedActivity1"))
            {
                // do some work that NestedActivity1 tracks
            }

            return items;
        }).WithName("GetPokemonsWithTags").WithOpenApi();

        group.MapGet("WithException", () =>
        {
            using var activity = Telemetry.ActivitySource.StartActivity("PokemonApi/WithException");

            try
            {
                throw new InvalidOperationException("Pokemon API exception");
            }
            catch (Exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Something went wrong!");
                throw;
            }
        }).WithName("GetPokemonsWithException").WithOpenApi();
    }
}

public record Pokemon(string Name, int Level);