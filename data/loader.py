from pkg_resources import resource_filename

import pandas

__available_continents = ['Africa', 'Asia', 'Europe', 'NorthAmerica', 'SouthAmerica']


def __load_file(filepath):
    local_ref_path = resource_filename(__name__, filepath)
    df = pandas.read_csv(local_ref_path)
    return df


def get_global_case_and_deaths_time_series_data():
    """
    This function reads the generated files from UW Time Series data
    at the directory location `UW Time Series/Global/World by country/`
    and uses the files:
    1. World_confirmed_sm.csv (for the cases data)
    2. World_deaths_sm.csv (for the deaths data)

    :return: (cases, deaths) Dataframes for the Global
    """
    local_dir_path = r'UW time series/Global/World by country/'
    case_file_name = 'World_confirmed_sm.csv'
    death_file_name = 'World_deaths_sm.csv'
    case_file_path = f'{local_dir_path}{case_file_name}'
    death_file_path = f'{local_dir_path}{death_file_name}'
    cases = __load_file(case_file_path)
    deaths = __load_file(death_file_path)
    return cases, deaths


def get_continent_specific_case_and_deaths_time_series_data(continent=None):
    """
    Available continents = ['Africa', 'Asia', 'Europe', 'NorthAmerica', 'SouthAmerica']
    :param continent:
    :return:
    """
    if continent is None:
        return None
    if continent not in __available_continents:
        return None
    local_dir_path = r'UW time series/Global/World by country/'
    case_file_name = f'{continent}_confirmed_sm.csv'
    death_file_name = f'{continent}_deaths_sm.csv'
    case_file_path = f'{local_dir_path}{case_file_name}'
    death_file_path = f'{local_dir_path}{death_file_name}'
    cases = __load_file(case_file_path)
    deaths = __load_file(death_file_path)
    return cases, deaths


def get_available_and_supported_continents():
    return __available_continents


def get_united_states_case_and_death_time_series_data(county=True):
    """
    Processes and returns the corresponding US COVID Cases and Deaths Time Series Data
    :param county: True or False to indicate if the returned data needs to be at County level
    per state or only at the state level.
    :return: (cases, deaths) DataFrames for the United States
    """
    local_dir_path = r'UW time series/Global/United States by county/'
    lookup_at = 'US'
    if not county:
        lookup_at = 'US_state'
    case_file_name = f'{lookup_at}_confirmed_sm.csv'
    death_file_name = f'{lookup_at}_deaths_sm.csv'
    case_file_path = f'{local_dir_path}{case_file_name}'
    death_file_path = f'{local_dir_path}{death_file_name}'
    cases = __load_file(case_file_path)
    deaths = __load_file(death_file_path)
    return cases, deaths
