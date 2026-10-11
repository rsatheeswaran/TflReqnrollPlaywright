Feature: Journey access details

@smoke
Scenario: View destination access information for a least walking journey
    Given the from location is "Leicester Square Underground Station"
    And the to location is "Covent Garden Underground Station"
    When the journey is planned
    And edit preferences is selected
    And routes with least walking is selected
    And the journey is updated
    And view details is selected
    Then the detailed journey time should match the result
    And complete access information at Covent Garden Underground Station is displayed
