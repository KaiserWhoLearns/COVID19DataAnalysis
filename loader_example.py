from data import loader


if __name__ == '__main__':
    cases, deaths = loader.get_global_case_and_deaths_time_series_data()
    print(cases)
    print(deaths)

    continents = loader.get_available_and_supported_continents()
    print(continents)

    for continent in continents:
        cases, deaths = loader.get_continent_specific_case_and_deaths_time_series_data(continent)
        print(cases)
        print(deaths)

