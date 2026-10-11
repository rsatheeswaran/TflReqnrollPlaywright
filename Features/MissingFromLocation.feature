Feature: Plan Journey with missing from location

@smoke
Scenario: Plan a journey with missing from location
	Given the from location is "" 
	And the to location is "Covent Garden Underground Station"
	When the journey is planned
	Then the error "The From field is required." should be displayed