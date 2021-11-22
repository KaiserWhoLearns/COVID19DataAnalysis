import matplotlib.pyplot as plt
import numpy
import pandas
import seaborn as sns
from scipy.stats import pearsonr

from data import loader
from helpers import geomap, heatmap
from helpers.compute import roundup

__NUMBER_OF_FILMS_IN_STRIP = 12


def plot_heatmap_case_death_correlation_pearson(cases, deaths, filter_items, name):
    fig, ax = plt.subplots(nrows=1, ncols=2, figsize=(20, 15), sharey=True)

    case_matrix = []
    death_matrix = []
    for i in filter_items:
        case_row = []
        death_row = []
        cases_x = cases[i]
        deaths_x = deaths[i]
        for j in filter_items:
            cases_y = cases[j]
            deaths_y = deaths[j]

            case_r, _case_p = pearsonr(cases_x, cases_y)
            death_r, _death_p = pearsonr(deaths_x, deaths_y)
            case_row.append(case_r)
            death_row.append(death_r)

        case_matrix.append(case_row)
        death_matrix.append(death_row)

    case_corr_df = pandas.DataFrame(data=case_matrix, index=filter_items, columns=filter_items)
    death_corr_df = pandas.DataFrame(data=death_matrix, index=filter_items, columns=filter_items)

    sns.heatmap(case_corr_df, annot=False, ax=ax[0], rasterized=True, cbar_kws={"orientation": "horizontal"})
    sns.heatmap(death_corr_df, annot=False, ax=ax[1], rasterized=True, cbar_kws={"orientation": "horizontal"})

    ax[0].set_title('Correlation of Cases per {}'.format(heatmap.get_normalized_per_population_value()))
    ax[1].set_title('Correlation of Deaths per {}'.format(heatmap.get_normalized_per_population_value()))

    plt.savefig(f'results/{name}_correlation_heatmap.png', bbox_inches='tight')


def plot_heatmap_daywise_correlation(cases,
                                     deaths,
                                     filter_items,  # Eg. County Names
                                     column_names,
                                     name,
                                     ):
    fig, ax = plt.subplots(nrows=1, ncols=2, figsize=(20, 15), sharey=True)

    case_data = []
    death_data = []

    for i in filter_items:
        c = cases[i]
        d = deaths[i]
        case_data.append(c)
        death_data.append(d)

    case_df = pandas.DataFrame(data=case_data, index=filter_items, columns=column_names)
    death_df = pandas.DataFrame(data=death_data, index=filter_items, columns=column_names)

    sns.heatmap(data=case_df, annot=False, ax=ax[0], rasterized=True, cbar_kws={"orientation": "horizontal"})
    sns.heatmap(data=death_df, annot=False, ax=ax[1], rasterized=True, cbar_kws={"orientation": "horizontal"})

    ax[0].set_title('Correlation of Cases per {}'.format(heatmap.get_normalized_per_population_value()))
    ax[1].set_title('Correlation of Deaths per {}'.format(heatmap.get_normalized_per_population_value()))

    plt.savefig(f'results/{name}_heatmap.png', bbox_inches='tight')


def plot_case_and_death_timeline_on_map(state_name='Washington', state_column_filter='Admin1'):
    state_fips_code = geomap.lookup_fips_code(state=state_name)
    state_geo_data = geomap.get_usa_county_level_data(state_fips_code=state_fips_code)
    usa_county_level_cases, usa_county_level_deaths = loader.get_united_states_case_and_death_time_series_data()
    state_cases = usa_county_level_cases[usa_county_level_cases[state_column_filter] == state_name]
    state_deaths = usa_county_level_deaths[usa_county_level_deaths[state_column_filter] == state_name]

    day_as_columns = list(range(len(state_cases.columns[len(heatmap.get_non_time_series_columns()):])))

    state_case_heatmap = heatmap.process_heatmap_data(state_cases, 'Admin2')
    state_death_heatmap = heatmap.process_heatmap_data(state_deaths, 'Admin2')

    state_case_matrix = []
    state_death_matrix = []

    state_case_matrix_columns = [f'CaseStripMean{d + 1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]
    state_death_matrix_columns = [f'DeathStripMean{d + 1}' for d in range(__NUMBER_OF_FILMS_IN_STRIP)]

    state_counties = state_geo_data['NAME'].to_list()
    dataset_counties = set(state_cases['Admin2'].to_list())

    plot_heatmap_daywise_correlation(state_case_heatmap,
                                     state_death_heatmap,
                                     state_counties,
                                     day_as_columns,
                                     state_name)

    plot_heatmap_case_death_correlation_pearson(
        state_case_heatmap,
        state_death_heatmap,
        state_counties,
        state_name,
    )

    for county in state_counties:
        case_result = [None] * __NUMBER_OF_FILMS_IN_STRIP
        death_result = [None] * __NUMBER_OF_FILMS_IN_STRIP
        if county not in state_case_heatmap:
            print(f"Ignoring {county}")
            state_case_matrix.append(case_result)
            state_death_matrix.append(death_result)
            continue
        cases_count = state_case_heatmap[county]
        death_count = state_death_heatmap[county]

        case_film_strip_chunks = numpy.array_split(cases_count, __NUMBER_OF_FILMS_IN_STRIP)
        death_film_strip_chunks = numpy.array_split(death_count, __NUMBER_OF_FILMS_IN_STRIP)

        for index, chunk_data in enumerate(case_film_strip_chunks):
            case_result[index] = numpy.mean(chunk_data)
        for index, chunk_data in enumerate(death_film_strip_chunks):
            death_result[index] = numpy.mean(chunk_data)

        state_case_matrix.append(case_result)
        state_death_matrix.append(death_result)

    case_matrix_df = pandas.DataFrame(data=state_case_matrix, columns=state_case_matrix_columns)
    death_matrix_df = pandas.DataFrame(data=state_death_matrix, columns=state_death_matrix_columns)

    df = pandas.concat([state_geo_data, case_matrix_df, death_matrix_df], axis=1)

    # Save intermediate data for the state timeline visualization!
    # df.to_csv(f'results/{state_name}.csv', header=True, index=False)

    calibrate_min = 0
    calibrate_case_max = roundup(case_matrix_df.max().max())
    calibrate_death_max = roundup(death_matrix_df.max().max())
    # print(f'Cases: {calibrate_min} --> {calibrate_case_max}')
    # print(f'Death: {calibrate_min} --> {calibrate_death_max}')

    fig, ax = plt.subplots(nrows=2, ncols=__NUMBER_OF_FILMS_IN_STRIP, figsize=(70, 20), sharex=True, sharey=True)
    # Plot all the cases in the first row
    for plot_row_index, (columns, vmax_filter) in enumerate([(state_case_matrix_columns, calibrate_case_max),
                                                             (state_death_matrix_columns, calibrate_death_max)]):
        for column_index, filter_name in enumerate(columns):
            df.plot(
                column=filter_name,
                legend=True,
                ax=ax[plot_row_index][column_index],
                vmin=calibrate_min,
                vmax=vmax_filter,
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
            ax[plot_row_index][column_index].set_axis_off()
    plt.tight_layout()
    plt.savefig(f'results/filmstrip_{state_name}.pdf', bbox_inches='tight')


if __name__ == '__main__':
    states = ['Washington', 'California', 'Oregon', 'Florida', 'New York', 'Arizona', 'North Dakota', 'South Dakota']
    # Examples!
    for state in states:
        print(f"Plotting case and death timeline for {state}")
        plot_case_and_death_timeline_on_map(state_name=state)
