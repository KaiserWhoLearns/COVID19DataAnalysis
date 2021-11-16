import datetime
import os.path

import geopandas
import pandas
from shapely.geometry import Point, Polygon
import matplotlib.pyplot as plt
import pandas as pd
from matplotlib import cm
from matplotlib.colors import ListedColormap, LinearSegmentedColormap

# 2020-04-27
COVARIANTS_FIRST_DATE = datetime.datetime(year=2020, month=4, day=27)
NUMBER_OF_DAYS_IN_A_WEEK = 7


def read_country_daily_timeline(filepath=r"data/UW time series/Global/World by country/World_deaths_sm.csv"):
    df = pandas.read_csv(filepath)
    return df


def find_covariant_country_cluster(country='India'):
    try:
        out_dir = f'data/covariants_country_clusters/{country}.csv'
        df = pandas.read_csv(out_dir)
        return df
    except FileNotFoundError:
        print(f'No Covariant data available for {country}.')


def find_first_non_zero(arr):
    return next((i for i, x in enumerate(arr) if x > 0.0), None)


def find_first_case_by_variant_in_country(country_df):
    variants_present = list(country_df.columns)
    variant_first_present = {}
    for variant in variants_present:
        data_series = country_df[variant]
        variant_first_present[variant] = find_first_non_zero(data_series.to_list())
    return variant_first_present


def find_deaths_by_country(deaths, cases):
    # Prepare for generating the outputs into the correct directory
    output_figure_path = 'results/cases_deaths_and_variants/'
    if not os.path.exists(output_figure_path):
        os.makedirs(output_figure_path)
    starting_column_headers = ['DataType', 'Admin2', 'Admin1', 'Admin0', 'Population', 'CaseCount']
    deaths_by_country = deaths[deaths['DataType'] == 'Deaths']
    cases_by_country = cases[cases['DataType'] == 'Confirmed']
    all_columns = deaths_by_country.columns
    columns_to_consider = all_columns[len(starting_column_headers):]
    columns_to_consider = [int(x.split('Day')[1].strip()) for x in columns_to_consider]
    countries = deaths_by_country['Admin0'].value_counts().keys().unique()
    cases_countries = cases_by_country['Admin0'].value_counts().keys().unique()
    all_country_data_match = (countries == cases_countries).all()
    if not all_country_data_match:
        print("Countries failed to match. Something went wrong in the data, please check")
        return

    for country in countries:
        print(f"Generating Data for {country}")
        fig, ax = plt.subplots(nrows=1, ncols=1)
        r = deaths_by_country[deaths_by_country['Admin0'] == country]
        row_data = []
        for index, row in r.iterrows():
            row_data = list(row[len(starting_column_headers):])
        ax.plot(columns_to_consider, row_data, label=f'{country} (Deaths)')
        c = cases_by_country[cases_by_country['Admin0'] == country]
        row_data = []
        for index, row in c.iterrows():
            row_data = list(row[len(starting_column_headers):])
        ax.plot(columns_to_consider, row_data, label=f'{country} (Cases)')

        # Overlap Covariant Data for the first case discovery counts
        ymin, ymax = ax.get_ylim()
        covariant_data = find_covariant_country_cluster(country)
        if covariant_data is not None:
            x = find_first_case_by_variant_in_country(covariant_data)
            day_approx_first_case = {}
            for k, v in x.items():
                if v is not None:
                    day_approx_first_case[k] = v * NUMBER_OF_DAYS_IN_A_WEEK
            colors = cm.get_cmap('tab20c', len(day_approx_first_case))
            line_styles = ['solid', 'dashed', 'dashdot', 'dotted']
            # Plot Vertical Lines of Variant Discovery in the Country
            for index, (k, v) in enumerate(day_approx_first_case.items()):
                plt.vlines(x=v, label=k, ymin=ymin, ymax=ymax, color=colors(index),
                           linestyle=line_styles[index % len(line_styles)])
        ax.set_xlabel('Day')
        ax.set_ylabel('Count')
        ax.set_title(f'Cases and Deaths in {country}')
        ax.legend(fancybox=True, ncol=3, bbox_to_anchor=(0, -0.5), loc='center left')
        plt.xticks(rotation=90)
        plt.savefig(f'{output_figure_path}{country}', bbox_inches='tight')
        plt.close('all')


if __name__ == '__main__':
    deaths = read_country_daily_timeline()
    cases = read_country_daily_timeline(filepath=r"data/UW time series/Global/World by country/World_confirmed_sm.csv")
    find_deaths_by_country(deaths, cases)
    # world = geopandas.read_file(geopandas.datasets.get_path('naturalearth_lowres'))
    # cities = geopandas.read_file(geopandas.datasets.get_path('naturalearth_cities'))
    # world = world[(world.pop_est > 0) & (world.name != "Antarctica")]
    # world['gdp_per_cap'] = world.gdp_md_est / world.pop_est
    # fig, ax = plt.subplots(1, 1)
    # world.plot(column='gdp_per_cap')
    # plt.show()