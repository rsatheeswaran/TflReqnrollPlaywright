Feature: Invalid journey locations

@smoke
Scenario: An invalid starting location does not produce a journey
    Given the invalid from location is "123456"
    And the to location is "Covent Garden Underground Station"
    When the journey is planned
    Then no journey results should be provided
