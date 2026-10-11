Feature: Journey Planner


@smoke
Scenario: Plan a valid journey
	Given the from location is "Leicester Square Underground Station"
	And the to location is "Covent Garden Underground Station"	
	When the journey is planned
	Then the result should be displayed for both walking and cycling time
