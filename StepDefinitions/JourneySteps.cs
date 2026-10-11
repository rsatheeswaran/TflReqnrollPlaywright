using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using TflReqnrollPlaywright.Support;
using static Microsoft.Playwright.Assertions;

namespace TflReqnrollPlaywright.StepDefinitions;

[Binding]
public sealed class JourneySteps
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    internal IPage Page =>
        _page ?? throw new InvalidOperationException("The browser page has not been initialized.");

    [BeforeScenario]
    public async Task OpenJourneyPlanner()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });
        _page = await _browser.NewPageAsync();

        await Page.GotoAsync("https://tfl.gov.uk/plan-a-journey/?cid=plan-a-journey");

        var cookieConsent = Page.GetByRole(AriaRole.Button, new()
        {
            Name = "Accept only essential cookies"
        });
        if (await cookieConsent.IsVisibleAsync())
        {
            await cookieConsent.ClickAsync();
        }
    }

    [AfterScenario]
    public async Task CloseJourneyPlanner()
    {
        try
        {
            if (_browser is not null)
            {
                await _browser.CloseAsync();
            }
        }
        finally
        {
            _playwright?.Dispose();
        }
    }

    [Given("the from location is {string}")]
    public Task GivenTheStartingPointIs(string location) =>
        SetLocationAsync("From location", location);

    [Given("the to location is {string}")]
    public Task GivenTheDestinationIs(string location) =>
        SetLocationAsync("To location", location);

    [Given("the invalid from location is {string}")]
    public async Task GivenTheInvalidFromLocationIs(string location)
    {
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "From location" }).FillAsync(location);
    }

    [When("the journey is planned")]
    public async Task WhenTheJourneyIsPlanned()
    {
        await Page.GetByRole(AriaRole.Button, new()
        {
            Name = "Plan my journey"
        }).PressAsync("Enter");
    }

    [Then("the result should be displayed")]
    public async Task ThenTheResultShouldBeDisplayed()
    {
        var resultHeading = Page.GetByRole(AriaRole.Heading, new()
        {
            Name = "Journey results"
        });
        await resultHeading.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    [Then("the result should be displayed for both walking and cycling time")]
    public async Task ThenWalkingAndCyclingTimesShouldBeDisplayed()
    {
        await ThenTheResultShouldBeDisplayed();
        foreach (var mode in new[] { "walking", "cycling" })
        {
            var time = Page.Locator($".journey-box.{mode} .journey-info");
            await Expect(time).ToBeVisibleAsync(new() { Timeout = 60000 });
            var minutes = JourneyDuration.ParseMinutes(await time.InnerTextAsync());
            var detailTime = Page.Locator($".{mode}-box .journey-time").First;
            await Expect(detailTime).ToContainTextAsync("min", new() { Timeout = 60000 });
            Assert.That(JourneyDuration.ParseMinutes(await detailTime.InnerTextAsync()), Is.EqualTo(minutes),
                $"The {mode} summary and route durations should match.");
        }
    }

    [Then("no journey results should be provided")]
    public async Task ThenNoJourneyResultsShouldBeProvided()
    {
        await ThenTheResultShouldBeDisplayed();
        await Expect(Page.Locator("#full-width-content")).ToContainTextAsync(
            "Journey planner could not find any results to your search. Please try again", new() { Timeout = 60000 });
        await Expect(Page.Locator(".publictransport-box")).ToHaveCountAsync(0);
    }

    [Then("the error {string} should be displayed")]
    public async Task ThenTheErrorShouldBeDisplayed(string expectedError)
    {
        var fromError = Page.Locator("#InputFrom-error");
        await fromError.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        Assert.That(await fromError.InnerTextAsync(), Is.EqualTo(expectedError));
    }

    [Then("the error {string} and {string} should be displayed")]
    public async Task ThenTheErrorsShouldBeDisplayed(string expectedFromError, string expectedToError)
    {
        var fromError = Page.Locator("#InputFrom-error");
        var toError = Page.Locator("#InputTo-error");

        await fromError.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        await toError.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        Assert.That(await fromError.InnerTextAsync(), Is.EqualTo(expectedFromError));
        Assert.That(await toError.InnerTextAsync(), Is.EqualTo(expectedToError));
    }

    private async Task SetLocationAsync(string fieldName, string location)
    {
        var field = Page.GetByRole(AriaRole.Combobox, new() { Name = fieldName });
        var searchText = location.Replace(" Underground Station", "", StringComparison.Ordinal);
        await field.FillAsync(searchText);

        if (string.IsNullOrWhiteSpace(location))
        {
            return;
        }

        var suggestion = Page.GetByRole(AriaRole.Option).Filter(new() { HasText = location });
        await suggestion.WaitForAsync();
        await Expect(Page.GetByRole(AriaRole.Option).First).ToContainTextAsync(location);
        await field.PressAsync("ArrowDown");
        await field.PressAsync("Enter");
        await Expect(field).ToHaveValueAsync(location);
    }
}
