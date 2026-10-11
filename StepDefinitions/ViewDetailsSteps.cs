using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using TflReqnrollPlaywright.Support;
using static Microsoft.Playwright.Assertions;

namespace TflReqnrollPlaywright.StepDefinitions;

[Binding]
public sealed class ViewDetailsSteps
{
    private readonly JourneySteps _journey;
    private int? _summaryMinutes;

    private ILocator Route => _journey.Page.Locator(".publictransport-box").First;

    public ViewDetailsSteps(JourneySteps journey) => _journey = journey;

    [When("view details is selected")]
    public async Task SelectViewDetails()
    {
        _summaryMinutes = JourneyDuration.ParseMinutes(await Route.Locator(".journey-time").InnerTextAsync());
        await Route.GetByText("View details", new() { Exact = true }).ClickAsync();
        await Expect(Route.Locator(".journey-detail-step").First).ToBeVisibleAsync();
    }

    [Then("the detailed journey time should match the result")]
    public async Task VerifyDetailedJourneyTime()
    {
        Assert.That(_summaryMinutes, Is.Not.Null, "View details must be selected before checking duration.");
        var durations = await Route.Locator(".journey-detail-step .duration").AllInnerTextsAsync();
        Assert.That(durations, Is.Not.Empty);
        var total = durations.Sum(JourneyDuration.ParseMinutes);
        Assert.That(total, Is.EqualTo(_summaryMinutes),
            "The sum of the detailed leg durations should match the result duration.");
    }

    [Then("complete access information at Covent Garden Underground Station is displayed")]
    public async Task VerifyDestinationAccessInformation()
    {
        var destinationStep = Route.Locator(".journey-detail-step").Filter(new()
        {
            Has = _journey.Page.Locator(".start-location[data-naptanid='940GZZLUCGN']")
        });
        await Expect(destinationStep.Locator(".location-name")).ToContainTextAsync("Covent Garden Underground Station");
        var access = destinationStep.Locator(".access-information");
        await Expect(access).ToBeVisibleAsync();
        foreach (var description in new[] { "Up stairs", "Up lift", "Level walkway" })
        {
            await Expect(access.GetByRole(AriaRole.Link, new() { Name = description, Exact = true }))
                .ToBeVisibleAsync();
        }
    }
}
