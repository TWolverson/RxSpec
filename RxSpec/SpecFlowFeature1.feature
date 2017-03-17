Feature: SpecFlowFeature1

Scenario: Files are copied
	Given the directory C:\Users\Tom\SpecFlow is empty
	When the file test.json is copied to directory C:\Users\Tom\SpecFlow
	Then 2 file is written to directory C:\Users\Tom\SpecFlow within 5 seconds
