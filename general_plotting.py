# EXISTING COLUMNS
# iso_code,continent,location,date,total_cases,new_cases,new_cases_smoothed,total_deaths,new_deaths,new_deaths_smoothed,total_cases_per_million,new_cases_per_million,
# new_cases_smoothed_per_million,total_deaths_per_million,new_deaths_per_million,new_deaths_smoothed_per_million,reproduction_rate,icu_patients,icu_patients_per_million,
# hosp_patients,hosp_patients_per_million,weekly_icu_admissions,weekly_icu_admissions_per_million,weekly_hosp_admissions,weekly_hosp_admissions_per_million,new_tests,
# total_tests,total_tests_per_thousand,new_tests_per_thousand,new_tests_smoothed,new_tests_smoothed_per_thousand,positive_rate,tests_per_case,tests_units,total_vaccinations,
# people_vaccinated,people_fully_vaccinated,total_boosters,new_vaccinations,new_vaccinations_smoothed,total_vaccinations_per_hundred,people_vaccinated_per_hundred,people_fully_vaccinated_per_hundred,
# total_boosters_per_hundred,new_vaccinations_smoothed_per_million,stringency_index,population,population_density,median_age,aged_65_older,aged_70_older,gdp_per_capita,
# extreme_poverty,cardiovasc_death_rate,diabetes_prevalence,female_smokers,male_smokers,handwashing_facilities,hospital_beds_per_thousand,life_expectancy,human_development_index,
# excess_mortality_cumulative_absolute,excess_mortality_cumulative,excess_mortality,excess_mortality_cumulative_per_million
#%%
import pandas as pd
import matplotlib.pyplot as plt

# Read general data
df = pd.read_csv("owid-covid-data.csv", usecols=["location", "date", "positive_rate", "people_vaccinated", "people_fully_vaccinated",
 "new_cases", "stringency_index", "new_deaths_smoothed"])
print(type(df["new_cases"]))

#%%
# countries = ["United States", "Italy", "India", "China"]
countries = ["United States", "Italy", "India"]
# Read data
for country in countries:
    country_data = df[df["location"] == country]
    country_data = country_data[country_data["people_vaccinated"] >= 0]

    ax1 = country_data.plot.scatter(x="people_fully_vaccinated", y="new_cases")
    ax1.set_title(country + " daily New Cases vs. People Fully Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="people_vaccinated", y="new_cases")
    ax1.set_title(country + " daily New Cases vs. People Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="people_vaccinated", y="new_deaths_smoothed")
    ax1.set_title(country + " daily Death vs. People Vaccinated")
    plt.show()

    ax1 = country_data.plot.scatter(x="stringency_index", y="new_deaths_smoothed")
    ax1.set_title(country + " daily Death vs. Stringency Index")
    plt.show()

    ax1 = country_data.plot.scatter(x="stringency_index", y="new_cases")
    ax1.set_title(country + " daily cases vs. Stringency Index")
    plt.show()

### Findings:
# 1. USA's data seems to have its own pattern compared to other countries. It may be better we look state by state
# 2. Although the new cases seems to decrease with vaccination count, there are a werid "peak" at the point of large vaccination count
#%%
countries = ["United States", "Italy", "India"]
# Read data
for country in countries:
    country_data = df[df["location"] == country]
    country_data = country_data[country_data["people_vaccinated"] >= 0]

    ax1 = country_data.plot.scatter(x="people_fully_vaccinated", y="positive_rate")
    ax1.set_title(country + " daily New Cases vs. Positive Rate per Test")
    plt.show()

# %%
