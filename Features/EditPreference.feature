Feature: Journey preferences

@smoke
Scenario: Update a journey to use routes with least walking
    Given the from location is "Leicester Square Underground Station"
    And the to location is "Covent Garden Underground Station"
    When the journey is planned
    And edit preferences is selected
    And routes with least walking is selected
    And the journey is updated
    Then the least walking preference should be applied
    And a positive public transport journey time should be displayed
