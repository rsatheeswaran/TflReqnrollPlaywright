using Microsoft.Playwright;
using Reqnroll;
using TflReqnrollPlaywright.Support;
using static Microsoft.Playwright.Assertions;

namespace TflReqnrollPlaywright.StepDefinitions;

[Binding]
public sealed class EditPreferenceSteps
{
    private readonly JourneySteps _journey;

    public EditPreferenceSteps(JourneySteps journey) => _journey = journey;

    [When("edit preferences is selected")]
    public async Task SelectEditPreferences()
    {
        await Expect(_journey.Page.Locator(".journey-box.walking .journey-info"))
            .ToBeVisibleAsync(new() { Timeout = 60000 });
        await _journey.Page.GetByRole(AriaRole.Button, new() { Name = "Edit preferences" }).ClickAsync();
    }

    [When("routes with least walking is selected")]
    public async Task SelectLeastWalking()
    {
        await _journey.Page.Locator("label[for='JourneyPreference_2']").ClickAsync();
        await Expect(_journey.Page.GetByRole(AriaRole.Radio, new() { Name = "Routes with least walking" }))
            .ToBeCheckedAsync();
    }

    [When("the journey is updated")]
    public async Task UpdateJourney()
    {
        await _journey.Page.GetByRole(AriaRole.Button, new() { Name = "Update journey" }).ClickAsync();
        await Expect(_journey.Page.Locator(".publictransport-box .journey-time").First)
            .ToBeVisibleAsync(new() { Timeout = 60000 });
    }

    [Then("the least walking preference should be applied")]
    public async Task VerifyLeastWalkingPreference()
    {
        await Expect(_journey.Page.Locator(".travel-preferences"))
            .ToContainTextAsync("Showing routes with least walking");
    }

    [Then("a positive public transport journey time should be displayed")]
    public async Task VerifyJourneyTime()
    {
        var time = _journey.Page.Locator(".publictransport-box .journey-time").First;
        await Expect(time).ToBeVisibleAsync(new() { Timeout = 60000 });
        JourneyDuration.ParseMinutes(await time.InnerTextAsync());
    }
}
