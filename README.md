# TfL Journey Planner Coding Challenge

C# UI tests for the [TfL Journey Planner](https://tfl.gov.uk/plan-a-journey/),
using Reqnroll for Gherkin bindings, NUnit as the test framework, and Playwright
for browser automation. The project targets .NET 8.

## Prerequisites and setup

- A .NET SDK capable of building .NET 8, and the .NET 8 runtime.
- PowerShell and access to the live TfL website.

From the project directory:

```powershell
dotnet restore
dotnet build
powershell -ExecutionPolicy Bypass -File .\bin\Debug\net8.0\playwright.ps1 install chromium
```

Package references are already declared in the project; there is no need to run
`dotnet add package` to set up an existing checkout.

## Running the tests

```powershell
dotnet test
dotnet test --filter "TestCategory=smoke"
dotnet test --logger "console;verbosity=detailed"
```

The last command includes individual step logs. `test.runsettings` reports NUnit
console output as information instead of warnings; actual warnings are not suppressed.

The browser currently runs in headed mode. In `StepDefinitions\JourneySteps.cs`,
set `Headless = true` for headless execution or `false` to see the browser.
`dotnet test --headed` is not a supported switch in this project.

## Automated coverage

There are six independent Gherkin scenarios:

| Requirement | Feature | Assertions |
|---|---|---|
| Valid journey, walking and cycling times | `PlanJourney.feature` | Both durations are positive and agree with the corresponding route durations. |
| Edit preferences to least walking | `EditPreference.feature` | The preference is applied and a positive public transport duration is displayed. |
| View details and destination access | `ViewDetails.feature` | Detailed leg durations sum to the result duration; Covent Garden arrival access shows Up stairs, Up lift and Level walkway. |
| Invalid location | `InvalidJourney.feature` | Invalid From text produces a no-results message and no public transport route cards. |
| No locations entered | `MissingBothLocations.feature` | Both required-field messages are displayed. |
| Extra negative scenario | `MissingFromLocation.feature` | The From required-field message is displayed with a valid destination. |

Each scenario starts with a new browser and its own page, so it does not depend
on another test having run first.

## Development decisions

- **Shared bindings:** Reqnroll bindings are global within the test assembly.
  Repeated Given/When phrases are implemented only once in `JourneySteps`.
  `EditPreferenceSteps` and `ViewDetailsSteps` implement only their unique steps
  and receive the same scenario-scoped `JourneySteps` instance through Reqnroll
  constructor injection. A feature does not need its own binding class.
- **Unique feature titles:** Separate feature files have distinct `Feature:`
  titles to avoid collisions between generated C# fixture classes.
- **Autocomplete:** Tests enter partial names (`Leicester Square` and
  `Covent Garden`), wait for the expected station suggestion, then select it using
  ArrowDown and Enter. They verify the selected full station value; they do not
  type the complete station name. The expected option must be first before
  keyboard selection occurs, otherwise the test fails explicitly.
- **Live durations:** The assignment does not specify fixed expected times.
  Durations are validated as positive minutes and compared between summaries
  and details where available, rather than assuming a constant journey time.
  This checks UI consistency, not independently calculated timetable accuracy.
- **Access assertions:** Expected destination access information is checked
  within the Covent Garden departure-from-platform/walk-out step of the first
  public transport route, not anywhere on the page. Changes to station access
  information require review of these expected values.
- **Synchronization:** Playwright locator waits and retrying assertions wait for
  asynchronously loaded results. The result heading alone is not evidence that
  the journey data has loaded. No fixed sleep or forced click is used.
- **Browser lifecycle:** Setup and cleanup are scenario-scoped. Playwright is
  disposed even if browser shutdown fails.
- **External dependency:** Tests use the live public service, so connectivity,
  timetable changes, station data and UI changes can affect their results.

### Playwright and ChromeDriver requirement

The brief mentions both Playwright and ChromeDriver. These are different
automation approaches: Playwright controls Chromium without ChromeDriver.
This implementation uses Playwright only. Whether a separate Selenium/
ChromeDriver implementation is required has not been clarified with the
challenge owner. This is an outstanding requirement clarification, not a claim
that Chromium satisfies a ChromeDriver requirement.

## Additional scenarios (not automated)

### Functional

1. Verify valid leaving/arriving dates and times, boundary dates, and past-time
   validation against the agreed business rules.
2. Earlier and later journey links return the appropriate adjacent journeys.
3. View Details/Hide Details and Map View/Hide Map toggle correctly.
4. Swap From and To locations and verify that the journey direction is reversed.
5. Verify autocomplete for ambiguous names, postcodes, no matches and keyboard
   selection, and clearing a previously selected location.
6. Verify an invalid destination and invalid values in both fields.
7. Verify accessibility preferences and transport-mode filters change the routes
   appropriately; present a clear message when no route meets the preferences.
8. Verify useful error feedback and recovery when the journey service is unavailable.

### Non-functional

1. Measure response time under representative usage against an agreed numerical
   threshold; distinguish service response time from total browser rendering time.
2. Assess keyboard navigation, focus order, field labels, validation announcements
   and screen-reader behaviour against the agreed WCAG target.
3. Assess colour contrast, zoom/reflow, mobile viewports and supported browsers.
4. Verify resilience under slow connections and intermittent service failures.
5. Verify cookie consent behaviour and that optional tracking follows the selected
   consent preferences.

## Further improvements

- Clarify the ChromeDriver requirement before submission.
- Move UI locators/actions into page objects and browser setup into a dedicated
  scenario-scoped driver as the suite grows.
- Add failure screenshots/traces, reporting and CI execution.
- Confirm numerical performance thresholds and exact expected access/timetable
  rules with the challenge owner.
- Publish the source to a public Git repository after checking for sensitive
  data and excluding generated build outputs and local IDE files.

## AI disclosure

GitHub Copilot was used to diagnose errors, generate and modify C# bindings and
test assertions, and assist with feature files and this README. Playwright
codegen was also used to record the original browser interactions. The author
is responsible for reviewing the generated code, understanding its behaviour
and validating the final submission.
