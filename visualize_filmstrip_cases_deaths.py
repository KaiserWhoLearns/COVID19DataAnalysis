import math

import matplotlib.pyplot as plt
import pandas
from matplotlib import cm
from matplotlib.colors import ListedColormap

from data import loader
from helpers import geomap, heatmap
import numpy

from helpers.compute import roundup

__NUMBER_OF_FILMS_IN_STRIP = 12


def plot_filmstrip(world, cases, deaths, name_column='name', filter_column='Admin0', q_name='world'):
    countries_in_the_world = world[name_column].to_list()
    country_wise_normalized_case_result = heatmap.process_heatmap_data(cases, filter_column)
    country_wise_normalized_death_result = heatmap.process_heatmap_data(deaths, filter_column)
    case_matrix = []
    death_matrix = []

    case_matrix_columns = [f'CaseStripMean{d+1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]
    death_matrix_columns = [f'DeathStripMean{d+1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]

    for country in countries_in_the_world:
        case_res = [None] * __NUMBER_OF_FILMS_IN_STRIP
        death_res = [None] * __NUMBER_OF_FILMS_IN_STRIP
        if country not in country_wise_normalized_case_result:
            # Both case_result and death_result have the same countries
            print(f"Ignoring {country}")
            case_matrix.append(case_res)
            death_matrix.append(death_res)
            continue
        cases_count = country_wise_normalized_case_result[country]
        deaths_count = country_wise_normalized_death_result[country]
        case_film_strip_chunks = numpy.array_split(cases_count, __NUMBER_OF_FILMS_IN_STRIP)
        deaths_film_strip_chunks = numpy.array_split(deaths_count, __NUMBER_OF_FILMS_IN_STRIP)
        for index, chunk_data in enumerate(case_film_strip_chunks):
            case_res[index] = numpy.mean(chunk_data)
        for index, chunk_data in enumerate(deaths_film_strip_chunks):
            death_res[index] = numpy.mean(chunk_data)
        case_matrix.append(case_res)
        death_matrix.append(death_res)

    case_matrix_df = pandas.DataFrame(data=case_matrix, columns=case_matrix_columns)
    deaths_matrix_df = pandas.DataFrame(data=death_matrix, columns=death_matrix_columns)

    lets_concat = pandas.concat([world, case_matrix_df, deaths_matrix_df], axis=1)

    calibrate_min = 0
    calibrate_case_max = roundup(case_matrix_df.max().max())
    calibrate_death_max = roundup(deaths_matrix_df.max().max(), nearest_ten=False)
    print(f'Cases: {calibrate_min} --> {calibrate_case_max}')
    print(f'Death: {calibrate_min} --> {calibrate_death_max}')

    fig, ax = plt.subplots(nrows=2, ncols=__NUMBER_OF_FILMS_IN_STRIP, figsize=(70, 20), sharex=True, sharey=True)
    # Plot all the cases first in the first row
    for column_index, filter_name in enumerate(case_matrix_columns):
        lets_concat.plot(
            column=filter_name,
            legend=True,
            ax=ax[0][column_index],
            vmin=calibrate_min,
            vmax=calibrate_case_max,
            cmap='viridis',
            missing_kwds={
                "color": "lightgrey",
                "edgecolor": "red",
                "hatch": "///",
                "label": "Missing values",
            },
            legend_kwds={
                'orientation': 'horizontal',
                'label': 'Cases per {}'.format(heatmap.get_normalized_per_population_value()),
            },
        )
        ax[0][column_index].set_axis_off()
    for column_index, filter_name in enumerate(death_matrix_columns):
        lets_concat.plot(
            column=filter_name,
            legend=True,
            ax=ax[1][column_index],
            cmap='viridis',
            vmin=calibrate_min,
            vmax=calibrate_death_max,
            missing_kwds={
                "color": "lightgrey",
                "edgecolor": "red",
                "hatch": "///",
                "label": "Missing values",
            },
            legend_kwds={
                'orientation': 'horizontal',
                'label': 'Deaths per {}'.format(heatmap.get_normalized_per_population_value()),
            },
        )
        ax[1][column_index].set_axis_off()
    plt.tight_layout()
    plt.savefig(f'results/filmstrip_{q_name}.pdf', bbox_inches='tight')


if __name__ == '__main__':
    world_cases, world_deaths = loader.get_global_case_and_deaths_time_series_data()
    world = geomap.get_world_data()
    world = world[(world.pop_est > 0) & (world.name != "Antarctica")]

    usa_cases, usa_deaths = loader.get_united_states_case_and_death_time_series_data(county=False)
    usa = geomap.get_usa_state_data(mainland_only=True)

    for (geodf, cases, deaths, main_filter, case_filter, name) in [
        (world, world_cases, world_deaths, 'name', 'Admin0', 'world'),
        (usa, usa_cases, usa_deaths, 'NAME', 'Admin1', 'usa'),
    ]:
        geo_data = set(geodf[main_filter].to_list())
        cov_data = set(cases[case_filter].to_list())

        common = cov_data.intersection(geo_data)
        diff = cov_data.difference(geo_data)
        print("Common: {}/{}".format(len(common), max(len(geo_data), len(cov_data))))
        print("Diff: {}/{}".format(len(diff), max(len(geo_data), len(cov_data))))

        print(diff)
        plot_filmstrip(geodf, cases, deaths, main_filter, case_filter, name)
