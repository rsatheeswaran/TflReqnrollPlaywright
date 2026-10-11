Feature: Plan Journey with missing both locations

@smoke
Scenario: Plan a journey with missing both locations
	Given the from location is "" 
	And the to location is ""
	When the journey is planned
	Then the error "The From field is required." and "The To field is required." should be displayed